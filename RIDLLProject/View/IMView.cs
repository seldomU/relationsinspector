using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using RelationsInspector.Extensions;
using RelationsInspector.Tweening;
using System;

namespace RelationsInspector
{
	public enum EntityWidgetType { Rect, Circle };

	public class EntityDrawContext
	{
		public Vector2 position;
		public bool isTarget;
		public bool isSelected;
		public bool isUnexlored;
		public EntityWidgetType widgetType;
		public EntityWidgetStyle style;
	}

	// Handles graph drawing and graph-related event handling
	internal class IMView<T, P> : IGraphView<T, P> where T : class
	{
		Transform2d transform;
		Graph<T, P> graph;
		Dictionary<T, Rect> entityDrawerBounds;
		Dictionary<Relation<T, P>, Rect> edgeMarkerBounds;

		Rect minimapRect;
		Transform2d minimapTransform;

		IRelationDrawer<T, P> tagDrawer;
		EdgePlacementProvider getEdgePlacement;

		T draggedEntity;
		HashSet<T> entitySelection;
		HashSet<T> dragEdgeSource;
		LinkedList<T> drawOrdered;
		IViewParent<T, P> parent;
		T hoverEntity;
		Relation<T, P> hoverRelation;

		EntityWidgetType entityWidgetType;
		const string PrefsKeyLayout = "IMViewLayout";
		const EntityWidgetType defaultWidgetType = EntityWidgetType.Rect;

		EntityDrawContext drawContext = new EntityDrawContext();    // local to Draw(). part of the state only to save allocs

		// rect selection
		Vector2 selectionRectOrigin;
		bool selectionRectActive;

		public IMView( Graph<T, P> graph, IViewParent<T, P> parent )
		{
			this.graph = graph;
			this.parent = parent;

			entityDrawerBounds = new Dictionary<T, Rect>();
			edgeMarkerBounds = new Dictionary<Relation<T, P>, Rect>();

			entitySelection = new HashSet<T>();
			dragEdgeSource = new HashSet<T>();

			// draw the targets last
			drawOrdered = new LinkedList<T>( graph.Vertices );
			foreach ( var target in graph.Vertices.Where( parent.IsSeed ) )
			{
				drawOrdered.Remove( target );
				drawOrdered.AddLast( target );
			}

			// initialize the drawers
			tagDrawer = new BasicRelationDrawer<T, P>();

			entityWidgetType = (EntityWidgetType) GUIUtil.GetPrefsInt( PrefsKeyLayout, (int) defaultWidgetType );
			InitEntityWidget();

			// make the graph fill the view
			transform = GetOptimalTransform();
		}

		void InitEntityWidget()
		{
			switch ( entityWidgetType )
			{
				case EntityWidgetType.Rect:
					getEdgePlacement = StraightRectPlacementProvider.GetEdgePlacement;
					break;
				default:
					getEdgePlacement = StraightCirclePlacementProvider.GetEdgePlacement;
					break;
			}
		}

		public void FitViewRectToGraph( bool immediately )
		{
			if ( immediately )
			{
				transform = GetOptimalTransform();
				return;
			}

			// viewRect: canvas shrunk down to 80%
			Rect viewRect = parent.GetViewRect();
			viewRect = Util.CenterRect( viewRect.center, viewRect.GetExtents() * 0.8f );

			// find graphRect (logical coords)
			var logicalPositions = graph.VerticesData.Values.Select( v => v.pos );
			Rect logicalPosBounds = Util.GetBounds( logicalPositions ).ClampExtents( 0.01f, 0.01f, float.MaxValue, float.MaxValue );

			var logicalExtents = logicalPosBounds.GetExtents();
			var viewExtents = viewRect.GetExtents();

			// fit logical extents into display extents
			var fullScale = new Vector2(
				viewExtents.x / logicalExtents.x,
				viewExtents.y / logicalExtents.y
				);

			var targetScale = transform.scale.Clamp( fullScale * 0.5f, fullScale );
			bool scaleChange = targetScale != transform.scale;
			var targetTransform = new Transform2d( transform.translation, targetScale, transform.rotation );
			var targetViewGraphBounds = targetTransform.Apply( logicalPosBounds );
			float xOver = targetViewGraphBounds.xMax - viewRect.xMax;
			float xUnder = targetViewGraphBounds.xMin - viewRect.xMin;
			float yOver = targetViewGraphBounds.yMax - viewRect.yMax;
			float yUnder = targetViewGraphBounds.yMin - viewRect.yMin;

			var shift = new Vector2(
				xOver > 0 ? xOver : xUnder < 0 ? xUnder : 0,
				yOver > 0 ? yOver : yUnder < 0 ? yUnder : 0 );
			bool translateChanged = shift != Vector2.zero;

			if ( scaleChange || translateChanged )
			{
				targetTransform.translation -= shift;
				Tweener.gen.Add( new Tween<Transform2d>( t => transform = t, 0.2f, TweenUtil.Transform2( transform, targetTransform, TwoValueEasing.Linear ) ) );
			}
		}

		// set the transform such that it can display the whole graph in the parent rect
		public Transform2d GetOptimalTransform()
		{
			var entityPositions = graph.VerticesData.Values.Select( v => v.pos );

			// use only the center area of the view rect for displaying the graph
			Rect viewRect = parent.GetViewRect();
			Rect graphRect = Util.CenterRect( viewRect.center, viewRect.GetExtents() * 0.7f );

			return ViewUtil.FitPointsIntoRect( entityPositions, graphRect );
		}

		IEnumerable<Tuple<IEnumerable<Relation<T, P>>, IEnumerable<Relation<T, P>>>> GetEdgesPerEntityPair()
		{
			var visitedPairs = new Dictionary<T, HashSet<T>>();

			foreach ( var entity in graph.Vertices )
			{
				var correspondents = graph.VerticesData[ entity ].GetCorrespondents();
				foreach ( var correspondent in correspondents )
				{
					// avoid doublets
					if ( visitedPairs.ContainsKey( correspondent ) &&
						visitedPairs[ correspondent ].Contains( entity ) )
						continue;

					// mark pair as covered 
					if ( !visitedPairs.ContainsKey( entity ) )
						visitedPairs[ entity ] = new HashSet<T>();
					visitedPairs[ entity ].Add( correspondent );

					yield return new Tuple<IEnumerable<Relation<T, P>>, IEnumerable<Relation<T, P>>>( graph.VerticesData[ entity ].InEdges.Get( correspondent ), graph.VerticesData[ entity ].OutEdges.Get( correspondent ) );
				}
			}
		}

		public void OnRemovedEntity( T entity )
		{
			entitySelection.Remove( entity );

			if ( draggedEntity == entity )
				draggedEntity = null;

			dragEdgeSource.Remove( entity );

			if ( hoverEntity == entity )
				hoverEntity = null;
		}

		public void OnRemovedRelation( Relation<T, P> relation )
		{
			if ( hoverRelation == relation )
				hoverRelation = null;
		}

		public bool ClearMissingRefs()
		{
			bool hasMissingRef = false;

			// no need to re-write null, but it does no harm either
			if ( draggedEntity != null && !graph.Vertices.Contains( draggedEntity ) )
			{
				draggedEntity = null;
				hasMissingRef = true;
			}

			// no need to re-write null, but it does no harm either
			if ( hoverEntity != null && !graph.Vertices.Contains( hoverEntity ) )
			{
				hoverEntity = null;
				hasMissingRef = true;
			}

			hasMissingRef |= RemoveNoneVertices( entitySelection );
			hasMissingRef |= RemoveNoneVertices( drawOrdered );
			hasMissingRef |= RemoveNoneVertices( dragEdgeSource );

			return hasMissingRef;
		}

		bool RemoveNoneVertices( ICollection<T> collection )
		{
			bool hasBadEntries = collection.Any( x => !graph.ContainsVertex( x ) );
			collection.RemoveWhere( x => !graph.ContainsVertex( x ) );
			return hasBadEntries;
		}

		public void OnToolbarGUI()
		{
			EditorGUI.BeginChangeCheck();
			entityWidgetType = (EntityWidgetType) GUIUtil.EnumToolbar( "", entityWidgetType, EditorStyles.miniButton );
			if ( EditorGUI.EndChangeCheck() )
			{
				GUIUtil.SetPrefsInt( PrefsKeyLayout, (int) entityWidgetType );
				// init entity widget and edge placement provider
				InitEntityWidget();
				parent.RepaintView();
			}
		}

		public void Draw()
		{
			// entities might have been destroyed externally
			if ( ClearMissingRefs() )
			{
				parent.RepaintView();
				return;
			}

			// get draw styles
			var skin = SkinManager.GetSkin();
			var relationDrawerStyle = skin.relationDrawer;

			// draw edges
			edgeMarkerBounds.Clear();
			foreach ( var edges in GetEdgesPerEntityPair() )
			{
				// get a representative edge
				var toEdges = edges._1;
				var fromEdges = edges._2;
				var repEdge = toEdges.Any() ? toEdges.First() : fromEdges.First();
				var swapPlacement = toEdges.Any();  // if our representative is a from-edge, swap placement start and end pos

				EdgePlacement placement;

				try
				{
					placement = GetEdgePlacement( repEdge );
				}
				catch ( System.Exception )
				{
					break;
				}

				if ( swapPlacement )
					placement = placement.Swap();

				bool isSelfEdge = ( repEdge.Source == repEdge.Target );
				bool highlight = entitySelection.Contains( repEdge.Source ) || entitySelection.Contains( repEdge.Target );

				var markerRects = tagDrawer.DrawRelation( toEdges, fromEdges, placement, isSelfEdge, highlight, true, relationDrawerStyle, parent.GetBackend().GetRelationColor );
				edgeMarkerBounds.UnionWith( markerRects );
			}

			// draw entities
			entityDrawerBounds.Clear();

			// include newly added entities in drawOrdered
			foreach ( var newEntity in graph.Vertices.Except( drawOrdered ) )
				drawOrdered.AddLast( newEntity );

			foreach ( var entity in drawOrdered )
				DrawEntity( entity );

			// draw the edge that is being created
			var fakeTargetRect = Util.CenterRect( Event.current.mousePosition, 1, 1 );
			foreach ( var source in dragEdgeSource )
			{
				Rect sourceRect = entityDrawerBounds[ source ];
				bool isSelfEdge = sourceRect.Contains( Event.current.mousePosition );
				var targetRect = isSelfEdge ? sourceRect : fakeTargetRect;

				var placement = getEdgePlacement( sourceRect, targetRect, relationDrawerStyle.edgeGapSize );
				tagDrawer.DrawPseudoRelation( placement, isSelfEdge, relationDrawerStyle );
			}

			// draw the selection rect
			if ( selectionRectActive )
			{
				var rect = Util.GetBounds( new[] { Event.current.mousePosition, selectionRectOrigin } );
				if ( Event.current.type == EventType.Repaint )
					GUI.skin.GetStyle( "SelectionRect" ).Draw( rect, GUIContent.none, false, false, false, false );
			}

			if ( Event.current.type == EventType.Repaint )
				DrawHoverItemTooltip();

			if ( Settings.Instance.showMinimap )
				DrawMinimap();
		}

		void DrawHoverItemTooltip()
		{
			if ( hoverEntity != null )
				DrawTooltip( parent.GetBackend().GetEntityTooltip( hoverEntity ), entityDrawerBounds[ hoverEntity ] );
			else if ( hoverRelation != null )
				DrawTooltip( parent.GetBackend().GetTagTooltip( hoverRelation.Tag ), edgeMarkerBounds[ hoverRelation ] );
		}

		void DrawTooltip( string tooltip, Rect parentItemRect )
		{
			if ( string.IsNullOrEmpty( tooltip ) )
				return;

			var content = new GUIContent( tooltip );
			var style = SkinManager.GetSkin().tooltipStyle;
			var contentSize = style.CalcSize( content );

			var tooltipRectCenter = FindTooltipRectPlacementInView( parentItemRect, contentSize, 20 );

			var contentRect = Util.CenterRect( tooltipRectCenter, contentSize );

			// draw outline
			EditorGUI.DrawRect( contentRect.AddBorder( 1f ), Color.black );

			EditorGUI.DrawRect( contentRect, SkinManager.GetSkin().windowColor );
			style.Draw( contentRect, content, 0 );
		}

		Vector2 FindTooltipRectPlacementInView( Rect parentRect, Vector2 tooltipExtents, float yOffset )
		{
			// work with a slightly more narrow viewRect, so that the tooltip rect stays away from the border
			var fullviewRect = parent.GetViewRect();
			var viewRect = new Rect( fullviewRect.xMin + 20, fullviewRect.yMin, fullviewRect.width - 2 * 20, fullviewRect.height );

			// get the tooltip's distance towards the viewRect border

			// distance to bottom border if placed below parent
			var clearanceYBottom = viewRect.yMax - (parentRect.yMax + yOffset + tooltipExtents.y);
			// distance to top border, if placed above parent
			var clearanceYTop = (parentRect.yMin - yOffset - tooltipExtents.y) - viewRect.yMin;
			// distance to right border
			var clearanceXRight = viewRect.xMax - ( parentRect.center.x + tooltipExtents.x / 2 );
			// distance to left border
			var clearanceXLeft = ( parentRect.center.x - tooltipExtents.x / 2 ) - viewRect.xMin;

			// place tooltip below parent if it fits into the window. 
			// If it does not fit, pick the side where more of the tooltip is visible.
			bool placeBottom = clearanceYBottom > 0 || clearanceYBottom > clearanceYTop;
			float xOffset = 0;
			if ( clearanceXLeft < 0 ) xOffset += -clearanceXLeft;	// shift to the right
			if ( clearanceXRight < 0 ) xOffset += clearanceXRight;  // shift to the left
			
			// have the tooltip leave the window when the parent does
			xOffset = Mathf.Clamp( xOffset, -( parentRect.width / 2 + tooltipExtents.x / 2 ), ( parentRect.width / 2 + tooltipExtents.x / 2 ) );

			return parentRect.center + new Vector2( xOffset, ( yOffset + tooltipExtents.y / 2 ) * ( placeBottom ? 1 : -1 ) );
		}

		void DrawEntity( T entity )
		{
			var vertexData = graph.VerticesData[ entity ];
			drawContext.position = transform.Apply( vertexData.pos );
			drawContext.isTarget = parent.IsSeed( entity );
			drawContext.isSelected = entitySelection.Contains( entity );
			drawContext.isUnexlored = vertexData.unexplored;
			drawContext.widgetType = entityWidgetType;
			drawContext.style = SkinManager.GetSkin().entityWidget;

			entityDrawerBounds[ entity ] = parent.GetBackend().DrawContent( entity, drawContext );
		}

		void DrawMinimap()
		{
			var entityViewPositions = graph.VerticesData.Values.Select( data => transform.Apply( data.pos ) );
			var style = SkinManager.GetSkin().minimap;
			Rect drawRect = parent.GetViewRect();
			minimapRect = Minimap.GetRect( SkinManager.GetSkin().minimap, Settings.Instance.minimapLocation, drawRect );
			minimapTransform = Minimap.Draw( entityViewPositions, minimapRect, drawRect, false, style );
		}

		EdgePlacement GetEdgePlacement( Relation<T, P> edge )
		{
			if ( edge == null )
				throw new System.ArgumentNullException( "edge" );

			if ( !entityDrawerBounds.ContainsKey( edge.Source ) || !entityDrawerBounds.ContainsKey( edge.Target ) )
				throw new System.Exception( "missing bounds for edge vertices" );


			var sourceBounds = entityDrawerBounds[ edge.Source ];
			var targetBounds = entityDrawerBounds[ edge.Target ];
			return getEdgePlacement( sourceBounds, targetBounds, SkinManager.GetSkin().relationDrawer.edgeGapSize );
		}

		T GetEntityAtPosition( Vector2 position )
		{
			return drawOrdered
				.FastReverse()
				.FirstOrDefault( ent => entityDrawerBounds[ ent ].Contains( position ) );
		}

		Relation<T, P> GetEdgeAtPosition( Vector2 position )
		{
			foreach ( var pair in edgeMarkerBounds )
				if ( pair.Value.Contains( position ) )
					return pair.Key;
			return null;
		}

		#region event handling

		public void HandleEvent( Event ev )
		{
			switch ( ev.rawType )
			{
				case EventType.mouseUp:
					draggedEntity = null;
					selectionRectActive = false;
					parent.RepaintView();
					break;
			}

			switch ( ev.type )
			{
				case EventType.mouseDown:

					// clicked on minimap -> focos camera on click pos
					if ( Settings.Instance.showMinimap && minimapRect.Contains( ev.mousePosition ) )
					{
						SetCenter( minimapTransform.Revert( ev.mousePosition ) );
						break;
					}

					var clickEntity = GetEntityAtPosition( ev.mousePosition );
					if ( clickEntity != null )
					{
						foreach ( var source in dragEdgeSource )
							parent.GetBackend().CreateRelation( source, clickEntity );
						dragEdgeSource = new HashSet<T>();

						if ( ev.button == 0 )   // left click
						{
							draggedEntity = clickEntity;

							// update selection
							bool controlHeld = ( ev.modifiers & EventModifiers.Control ) != 0;
							if ( controlHeld )
							{
								if ( entitySelection.Contains( clickEntity ) )
									entitySelection.Remove( clickEntity );
								else
									entitySelection.Add( clickEntity );
							}
							else
								entitySelection = new HashSet<T>( new[] { clickEntity } );

							OnEntitySelectionChange();
							parent.RepaintView();
						}
					}
					else // clickEntity == null
					{
						var clickEdge = GetEdgeAtPosition( ev.mousePosition );
						if ( clickEdge == null ) // clickEdge == null && clickEntity == null
						{
							if ( ev.button != 1 )   // not a right click
							{
								bool controlHeld = ( ev.modifiers & EventModifiers.Control ) != 0;
								if ( !controlHeld )
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

				case EventType.ContextClick:
					var ccEntity = GetEntityAtPosition( ev.mousePosition );
					if ( ccEntity != null )
					{
						if ( !entitySelection.Contains( ccEntity ) )
							entitySelection = new HashSet<T>() { ccEntity };

						HandleEntityContextClick( entitySelection );
						Event.current.Use();
						break;
					}

					var ccEdge = GetEdgeAtPosition( ev.mousePosition );
					if ( ccEdge != null )
					{
						HandleRelationContextClick( ccEdge );
						Event.current.Use();
						break;
					}
					break;

				case EventType.MouseMove:

					// update hover item and repaint if it changed
					var newHoverEntity = GetEntityAtPosition( ev.mousePosition );
					var newHoverRelation = ( newHoverEntity != null ) ? null : GetEdgeAtPosition( ev.mousePosition );
					bool hoverChange = newHoverEntity != hoverEntity || newHoverRelation != hoverRelation;
					hoverEntity = newHoverEntity;
					hoverRelation = newHoverRelation;

					bool doRepaint = hoverChange;   // movement over the hover-item requires a repainted tooltip

					// also repaint if we are dragging an edge
					doRepaint |= dragEdgeSource.Any();

					if ( doRepaint )
						parent.RepaintView();
					break;

				case EventType.MouseUp:
					draggedEntity = null;
					selectionRectActive = false;
					parent.RepaintView();
					break;

				case EventType.MouseDrag:

					if ( draggedEntity != null )
					{
						parent.MoveEntity( draggedEntity, transform.RevertScale( ev.delta ) );
					}
					else if ( selectionRectActive ) // update selection
					{
						// GetBounds ensures that xmin < xmax and ymin < ymax
						var selectionRect = Util.GetBounds( new[] { ev.mousePosition, selectionRectOrigin } );
						var touchedEntityBounds = entityDrawerBounds.Where( pair => selectionRect.Intersects( pair.Value ) );
						entitySelection = new HashSet<T>( touchedEntityBounds.Select( pair => pair.Key ) );
						OnEntitySelectionChange();
					}
					else
					{
						Shift( ev.delta );
					}

					parent.RepaintView();
					break;

				case EventType.ScrollWheel:
					bool xZoom = ( ev.modifiers & EventModifiers.Control ) == 0;
					bool yZoom = ( ev.modifiers & EventModifiers.Shift ) == 0;
					bool zoomIn = Settings.Instance.invertZoom ? ev.delta.y > 0 : ev.delta.y < 0;

					var targetTransform = Zoom( transform, zoomIn, xZoom, yZoom, ev.mousePosition );
					Tweener.gen.Add( new Tween<Transform2d>( t => transform = t, 0.1f, TweenUtil.Transform2( transform, targetTransform, TwoValueEasing.Linear ) ) );  //.MoveTransform2dTo(transform, t=>Zoom(t, zoomIn, xZoom, yZoom, ev.mousePosition), 0.1f, true);

					ev.Use();
					parent.RepaintView();
					break;
			}
		}

		void HandleEntityContextClick( IEnumerable<T> entities )
		{
			var menu = new GenericMenu();

			// a vertex can be folded when it produces fold-vertices
			if ( entities.Any( e => FoldUtil.GetFoldVertices( graph, e, parent.IsSeed ).Any() ) )
			{
				menu.AddItem( new GUIContent( "Fold" ), false, () =>
				{
					foreach ( var ent in entities )
					{
						parent.GetAPI().FoldEntity( ent );
					}
				} );
			};

			// a vertex can be expanded when it is marked as unexplored 
			if ( entities.Any( e => graph.VerticesData[ e ].unexplored ) )
			{
				menu.AddItem( new GUIContent( "Expand" ), false, () =>
				{
					foreach ( var ent in entities )
					{
						parent.GetAPI().ExpandEntity( ent );
					}
				} );
			}

			parent.GetBackend().OnEntityContextClick( entities, menu );
			if ( menu.GetItemCount() > 0 )
				menu.ShowAsContext();
		}

		void HandleRelationContextClick( Relation<T, P> clickEdge )
		{
			var menu = new GenericMenu();
			parent.GetBackend().OnRelationContextClick( clickEdge.Copy(), menu );
			if ( menu.GetItemCount() > 0 )
				menu.ShowAsContext();
		}

		void OnEntitySelectionChange()
		{
			foreach ( var entity in entitySelection )
			{
				drawOrdered.Remove( entity );
				drawOrdered.AddLast( entity );
			}

			parent.GetBackend().OnEntitySelectionChange( entitySelection.ToArray() );
		}

		static Transform2d Zoom( Transform2d transform, bool zoomIn, bool affectX, bool affectY, Vector2 fixPosition )
		{
			// adjust the offset such that the window stays centered on the same graph position
			var fixPositionBase = transform.Revert( fixPosition );

			var targetTransform = new Transform2d( transform );
			if ( affectX )
				targetTransform.scale.x *= zoomIn ? 3f / 2 : 2f / 3;
			if ( affectY )
				targetTransform.scale.y *= zoomIn ? 3f / 2 : 2f / 3;

			var newfixPosition = targetTransform.Apply( fixPositionBase );
			targetTransform.translation += fixPosition - newfixPosition;
			return targetTransform;
		}

		void Shift( Vector2 delta )
		{
			transform.translation += delta;
			transform.translation = transform.translation.Clamp( GetTranslationBounds() );
		}

		Rect GetTranslationBounds()
		{
			// make sure the graph bounds don't leave the view rect entirely
			Rect graphBounds = Util.GetBounds( graph.VerticesData.Values.Select( v => v.pos ) );
			graphBounds = transform.ApplyScale( graphBounds );

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

		public Rect GetViewRect( Rect source )
		{
			return transform.Revert( source );
		}

		public Vector2 GetGraphPosition( Vector2 source )
		{
			return transform.Revert( source );
		}

		public void SetCenter( Vector2 newCenter )
		{
			var currentCenter = parent.GetViewRect().center;
			var offset = ( newCenter - currentCenter );
			transform.translation -= offset;
		}

		public void CreateEdge( IEnumerable<T> sourceEntities )
		{
			dragEdgeSource = new HashSet<T>( sourceEntities );
		}

		public void SelectEntityNodes( System.Predicate<object> doSelect )
		{
			entitySelection = new HashSet<T>( graph.Vertices.Where( v => doSelect( v ) ) );
			OnEntitySelectionChange();
		}
	}
}
