using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using RelationsInspector.Extensions;
using RelationsInspector.Tween;

namespace RelationsInspector
{
	public enum EntityWidgetType { Rect, Circle };

	public class EntityDrawContext
	{
		public Vector2 position;
		public bool isTarget;
		public bool isSelected;
		public EntityWidgetType widgetType;
		public EntityWidgetStyle style;
	}

	// Stores its own version of the graph. Handles entity- and tag drawing and transformations.
	internal class IMView<T, P> : IGraphView<T, P> where T : class
	{
		Transform2d transform;
		Graph<T, P> graph;
		Dictionary<T, Rect> entityDrawerBounds;
		Dictionary<Edge<T, P>, Rect> edgeMarkerBounds;

		IRelationDrawer<T,P> tagDrawer;
		IEdgePlacementProvider edgePlacementProvider;
		float edgeGapSize = 4;	// size of the gap between entity drawer bounds and edge

		T draggedEntity;
		HashSet<T> entitySelection;
		HashSet<T> dragEdgeSource;
		P dragEdgeTag;
		IViewParent<T, P> parent;
		T hoverEntity;

		EntityWidgetType entityWidgetType;
		const string PrefsKeyLayout = "IMViewLayout";
		const EntityWidgetType defaultWidgetType = EntityWidgetType.Rect;

		EntityDrawContext drawContext = new EntityDrawContext();

		// rect selection
		Vector2 selectionRectOrigin;
		bool selectionRectActive;

#if DEBUG
		bool showScale;
#endif

		public IMView(Graph<T,P> graph, IViewParent<T, P> parent )
		{
			this.graph = graph;
			this.parent = parent;

			entityDrawerBounds = new Dictionary<T, Rect>();
			edgeMarkerBounds = new Dictionary<Edge<T, P>, Rect>();

			entitySelection = new HashSet<T>();
			dragEdgeSource = new HashSet<T>();

			// initialize the drawers
			tagDrawer = new BasicRelationDrawer<T, P>();	//(IRelationDrawer<P>) System.Activator.CreateInstance(tagDrawerType);

			entityWidgetType = (EntityWidgetType)GUIUtil.GetPrefsInt(PrefsKeyLayout, (int)defaultWidgetType);
			InitEntityWidget();

			// make the graph fill the view
			FitViewRectToGraph();
		}

		void InitEntityWidget()
		{
			if (entityWidgetType == EntityWidgetType.Rect)
			{
				edgePlacementProvider = new StraightRectPlacementProvider();
			}
			else
			{
				edgePlacementProvider = new StraightCirclePlacementProvider();
			}
		}

		// set the transform such that it can display the whole graph in the parent rect
		public void FitViewRectToGraph()
		{
			var entityPositions = graph.VerticesData.Values.Select(v => v.pos);
			transform = ViewUtil.FitPointsIntoRect(entityPositions, parent.GetViewRect(), 0.3f);
		}

		IEnumerable<Tuple<IEnumerable<Edge<T, P>>, IEnumerable<Edge<T, P>>>> GetEdgesPerEntityPair()
		{
			var visitedPairs = new Dictionary<T, HashSet<T>>();

			foreach (var entity in graph.Vertices)
			{
				var correspondents = graph.VerticesData[entity].GetCorrespondents();
				foreach (var correspondent in correspondents)
				{
					// avoid doublets
					if (visitedPairs.ContainsKey(correspondent) && 
						visitedPairs[correspondent].Contains(entity))
						continue;

					// mark pair as covered 
					if (!visitedPairs.ContainsKey(entity))
						visitedPairs[entity] = new HashSet<T>();
					visitedPairs[entity].Add(correspondent);

					yield return new Tuple<IEnumerable<Edge<T, P>>, IEnumerable<Edge<T, P>>>(graph.VerticesData[entity].InEdges.Get(correspondent), graph.VerticesData[entity].OutEdges.Get(correspondent));
				}
			}
		}

		public void OnRemovedEntity(T entity)
		{
			entitySelection.Remove(entity);

			if (draggedEntity == entity)
				draggedEntity = null;

			dragEdgeSource.Remove(entity);

			if (hoverEntity == entity)
				hoverEntity = null;
		}

		public void ClearMissingRefs()
		{
			if (draggedEntity != null && draggedEntity.Equals(null))
				draggedEntity = null;
			if (hoverEntity != null && hoverEntity.Equals(null))
				hoverEntity = null;

			entitySelection.RemoveWhere(entity => Util.IsBadRef(entity));
			dragEdgeSource.RemoveWhere( entity => Util.IsBadRef(entity) );
		}

		public void OnToolbarGUI()
		{
			EditorGUI.BeginChangeCheck();
			entityWidgetType = (EntityWidgetType) GUIUtil.EnumToolbar("", entityWidgetType, EditorStyles.miniButton);
			if (EditorGUI.EndChangeCheck())
			{
				GUIUtil.SetPrefsInt(PrefsKeyLayout, (int)entityWidgetType);
				// init entity widget and edge placement provider
				InitEntityWidget();
				parent.RepaintView();
			}
#if DEBUG
			showScale = GUILayout.Toggle(showScale, "showScale");
			if (showScale)
			{
				GUILayout.Label(transform.scale.ToString());
			}
#endif
		}

		public void Draw( )
		{
			// entities might have been destroyed externally
			ClearMissingRefs();

			// get draw styles
			var relationDrawerStyle = parent.GetSkin().relationDrawer;
			var entityWidgetStyle = parent.GetSkin().entityWidget;
			
			// draw edges
			edgeMarkerBounds.Clear();
			foreach (var edges in GetEdgesPerEntityPair())
			{				
				// get a representative edge
				var toEdges = edges._1;
				var fromEdges = edges._2;
				var repEdge = toEdges.Any() ? toEdges.First() : fromEdges.First();
				var swapPlacement = toEdges.Any();	// if our representative is a from-edge, swap placement start and end pos

				EdgePlacement placement;
				
				try
				{
					placement = GetEdgePlacement(repEdge);
				}
				catch(System.Exception)
				{
					break;
				}

				if (swapPlacement)
					placement = placement.Swap();

				bool isSelfEdge = (repEdge.Source == repEdge.Target);
				bool highLight = entitySelection.Contains(repEdge.Source) || entitySelection.Contains(repEdge.Target);

				var markerRects = tagDrawer.DrawRelation(toEdges, fromEdges, placement, isSelfEdge, highLight, true, relationDrawerStyle, parent.GetBackend().GetRelationColor );
				edgeMarkerBounds.UnionWith(markerRects);
			}

			// draw entities
			entityDrawerBounds.Clear();
			foreach (var pair in graph.VerticesData)
			{
				T entity = pair.Key;
				// selected entity gets drawn seperately (after this)
				if (entitySelection.Contains(entity))
					continue;

				drawContext.position = transform.Apply(pair.Value.pos);
				drawContext.isTarget = parent.IsRoot(entity);
				drawContext.isSelected = false;
				drawContext.widgetType = entityWidgetType;
				drawContext.style = entityWidgetStyle;

				entityDrawerBounds[entity] = parent.GetBackend().DrawContent(entity, drawContext);
			}

			// draw selected entity
			foreach(var entity in entitySelection)
			{
				if (Util.IsBadRef(entity))
					continue;	// throw exception?

				drawContext.position = transform.Apply(graph.GetPos(entity));
				drawContext.isTarget = parent.IsRoot(entity);
				drawContext.isSelected = true;
				drawContext.widgetType = entityWidgetType;
				drawContext.style = entityWidgetStyle;

				entityDrawerBounds[entity] = parent.GetBackend().DrawContent(entity, drawContext);
			}

			// draw the edge that is being created
			var fakeTargetRect = Util.CenterRect(Event.current.mousePosition, 1, 1);
			foreach (var source in dragEdgeSource)
			{
				Rect sourceRect = entityDrawerBounds[source];
				bool isSelfEdge = sourceRect.Contains(Event.current.mousePosition);
				var targetRect = isSelfEdge ? sourceRect : fakeTargetRect;

				var placement = edgePlacementProvider.GetEdgePlacement(sourceRect, targetRect, edgeGapSize);
				tagDrawer.DrawPseudoRelation(placement, isSelfEdge, relationDrawerStyle);
			}

			// draw the selection rect
			if (selectionRectActive)
			{
				var rect = Util.GetBounds( new[] { Event.current.mousePosition, selectionRectOrigin } );
				var color = parent.GetSkin().windowColor;
				color.a *= 0.5f;
				if (Event.current.type == EventType.Repaint)
					GUI.skin.GetStyle("SelectionRect").Draw(rect, GUIContent.none, false, false, false, false);
			}

			// draw the tooltip
			if( hoverEntity != null )
			{
				var tooltip = parent.GetBackend().GetTooltip(hoverEntity);
				if ( !string.IsNullOrEmpty(tooltip) )
				{
					if(Event.current.type == EventType.Repaint)
					{
						var content = new GUIContent(tooltip);
						var size = GUI.skin.box.CalcSize(content);
						var contentRect = FindTooltipRect(Event.current.mousePosition, size);
						var contentPadding = new[] { 3f, 1f, 2f, 0f };					
						var bgColor = parent.GetSkin().windowColor;
						DrawPaddedLabel(content, contentRect, contentPadding, bgColor);
					}
				}
			}
		}

		// draw label with padding around the content rect
		static void DrawPaddedLabel(GUIContent label, Rect labelRect, float[] padding, Color bgColor, bool outLined = true)
		{
			Rect paddedRect = labelRect.AddBorder(padding[0], padding[1], padding[2], padding[3]);
			if (outLined)
				EditorGUI.DrawRect(paddedRect.AddBorder(1f), Color.black);

			EditorGUI.DrawRect(paddedRect, bgColor);
			GUI.Label(labelRect, label);
		}

		Rect FindTooltipRect(Vector2 mousePos, Vector2 extents)
		{
			Rect parentRect = parent.GetViewRect();
			var mousePosToRectMargin = new Vector2(0, 20);
			var rectCenter = mousePos + mousePosToRectMargin;
			rectCenter.y += extents.y / 2;

			return Util.CenterRect(rectCenter, extents);
		}

		EdgePlacement GetEdgePlacement(Edge<T, P> edge)
		{
			if (edge == null)
				throw new System.ArgumentNullException("edge");

			if (!entityDrawerBounds.ContainsKey(edge.Source) || !entityDrawerBounds.ContainsKey(edge.Target))
				throw new System.ArgumentException("missing bounds for edge vertices");

			var sourceBounds = entityDrawerBounds[edge.Source];
			var targetBounds = entityDrawerBounds[edge.Target];
			return edgePlacementProvider.GetEdgePlacement(sourceBounds, targetBounds, edgeGapSize);
		}

		public T GetEntityAtPosition(Vector2 position)
		{
			foreach(var pair in entityDrawerBounds)
				if( pair.Value.Contains(position) )
					return pair.Key;

			return null;
		}

		Edge<T, P> GetEdgeMarkerAtPosition(Vector2 position)
		{
			foreach (var pair in edgeMarkerBounds)
				if (pair.Value.Contains(position))
					return pair.Key;
			return null;
		}

		#region event handling

		public void HandleEvent(Event ev)
		{
			switch (ev.rawType)
			{
				case EventType.mouseUp:
					draggedEntity = null;
					selectionRectActive = false;
					parent.RepaintView();
					break;
			}

			switch (ev.type)
			{
				case EventType.mouseDown:
					var clickEntity = GetEntityAtPosition(ev.mousePosition);
					if (clickEntity != null)
					{
						foreach (var source in dragEdgeSource)
							parent.GetBackend().CreateRelation(source, clickEntity, dragEdgeTag);
						dragEdgeSource = new HashSet<T>();

						if (ev.button == 0)	// left click
						{
							draggedEntity = clickEntity;

							// update selection
							bool controlHeld = (ev.modifiers & EventModifiers.Control) != 0;
							if (controlHeld)
							{
								if (entitySelection.Contains(clickEntity))
									entitySelection.Remove(clickEntity);
								else
									entitySelection.Add(clickEntity);
							}
							else
								entitySelection = new HashSet<T>(new[] { clickEntity });

							if (OnEntitySelectionChange())
								parent.RepaintView();
						}
						else if (ev.button == 1)	// right click
						{
							parent.GetBackend().OnEntityContextClick( new[]{clickEntity} ); 
						}
					}
					else // clickEntity == null
					{
						var clickEdge = GetEdgeMarkerAtPosition(ev.mousePosition);
						if (clickEdge != null)
						{
							if (ev.button == 1)	// right click
							{
								parent.GetBackend().OnRelationContextClick( clickEdge.Source, clickEdge.Target, clickEdge.Tag );
								ev.Use();	// eat event, or else a new entity is created here
							}
						}
						else // clickEdge == null && clickEntity == null
						{
							if (ev.button == 1)	// right click
							{
								bool controlHeld = (ev.modifiers & EventModifiers.Control) != 0;
								if (controlHeld)
								{
									var entityPos = transform.Revert(ev.mousePosition);
									parent.GetBackend().CreateEntity(entityPos);
								}
							}
							else
							{
								bool controlHeld = (ev.modifiers & EventModifiers.Control) != 0;
								if (!controlHeld)
									entitySelection = new HashSet<T>();

								// stop edge creation
								dragEdgeSource = new HashSet<T>();								

								// start rect selection
								selectionRectActive = true;
								selectionRectOrigin = ev.mousePosition;

								parent.RepaintView();
							}
						}
					}
					break;

				case EventType.MouseMove:

					// update hover entity and repaint if it changed
					var newHoverEntity = GetEntityAtPosition(ev.mousePosition);
					bool doRepaint = hoverEntity!=null || hoverEntity != newHoverEntity;	// movement over the hover-entity requires a repainted tooltip
					hoverEntity = newHoverEntity;

					// also repaint if we are dragging an edge
					doRepaint |= dragEdgeSource.Any();

					if (doRepaint)
						parent.RepaintView();
					break;

				case EventType.MouseUp:
					draggedEntity = null;
					selectionRectActive = false;
					parent.RepaintView();
					break;

				case EventType.MouseDrag:

					if (draggedEntity != null)
					{
						parent.MoveEntity(draggedEntity, transform.RevertScale(ev.delta));
					}
					else if (selectionRectActive)	// update selection
					{
						// GetBounds ensures that xmin < xmax and ymin < ymax
						var selectionRect = Util.GetBounds(new[] { ev.mousePosition, selectionRectOrigin });
						var touchedEntityBounds = entityDrawerBounds.Where(pair => selectionRect.Intersects(pair.Value));
						entitySelection = new HashSet<T>(touchedEntityBounds.Select(pair => pair.Key));
						OnEntitySelectionChange();
					}
					else
					{
						Shift(ev.delta);
					}

					parent.RepaintView();
					break;

				case EventType.ScrollWheel:
					bool zoomIn = ev.delta.y > 0;
					var zoomedTransform = Zoom(transform, zoomIn, ev.mousePosition);
					Tweener.gen.MoveTransform2dTo(transform, t=>Zoom(t, zoomIn, ev.mousePosition), 0.1f, true);	//zoomedTransform, 0.1f, true);

					ev.Use();
					parent.RepaintView();
					break;
			}
		}

		void DragEdge(IEnumerable<T> vertices, P tag)
		{
			dragEdgeSource = new HashSet<T>(vertices);
			dragEdgeTag = tag;
		}

		bool OnEntitySelectionChange()
		{
			parent.GetBackend().OnEntitySelectionChange( entitySelection.ToArray() );
			return true;
		}

		static Transform2d Zoom(Transform2d transform, bool zoomIn, Vector2 fixPosition)
		{
			// adjust the offset such that the window stays centered on the same graph position
			var fixPositionBase = transform.Revert(fixPosition);

			var targetTransform = new Transform2d(transform);
			targetTransform.scale *= zoomIn ? 3f / 2 : 2f / 3;

			var newfixPosition = targetTransform.Apply(fixPositionBase);
			targetTransform.translation += fixPosition - newfixPosition;
			return targetTransform;
		}

		void Shift(Vector2 delta)
		{
			transform.translation += delta;
			transform.translation = transform.translation.Clamp(GetTranslationBounds());
		}

		Rect GetTranslationBounds()
		{
			// make sure the graph bounds don't leave the view rect entirely
			Rect graphBounds = Util.GetBounds( graph.VerticesData.Values.Select(v => v.pos) );
			graphBounds = transform.ApplyScale(graphBounds);

			Rect viewBounds = parent.GetViewRect();

			// distance between centers can't be more than 1/2 of their combined extents
			return Util.CenterRect(
				viewBounds.center - graphBounds.center,
				viewBounds.width + graphBounds.width,
				viewBounds.height + graphBounds.height
				);

			/*
			// alternative
			float xmin = viewBounds.xMin - graphBounds.xMax;
			float xmax = viewBounds.xMax - graphBounds.xMin;
			float ymin = viewBounds.yMin - graphBounds.yMax;
			float ymax = viewBounds.yMax - graphBounds.yMin;
			*/
		}

		#endregion

		public Rect GetViewRect(Rect source)
		{ 
			return transform.Revert( source ); 
		}

		public Vector2 GetGraphPosition(Vector2 source)
		{ 
			return transform.Revert( source ); 
		}

		public void OnWindowSelectionChange() { }

		public void SetCenter(Vector2 newCenter)
		{
			var currentCenter = GetViewRect(parent.GetViewRect()).center;
			var offset = (newCenter - currentCenter) * transform.scale.x;
			transform.translation -= offset;
		}

		public void CreateEdge(IEnumerable<T> sourceEntities, P tag)
		{
			dragEdgeSource = new HashSet<T>(sourceEntities);
			dragEdgeTag = tag;
		}

		public void SelectEntityNodes(System.Predicate<object> doSelect)
		{
			entitySelection = new HashSet<T>( graph.Vertices.Where( v => doSelect(v) ) );
			OnEntitySelectionChange();
		}
	}
}
