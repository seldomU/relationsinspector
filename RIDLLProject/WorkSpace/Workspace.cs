using RelationsInspector;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using RelationsInspector.Extensions;
using RelationsInspector.Tweening;
using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Collections;

namespace RelationsInspector
{
	internal class Workspace<T,P> : IWorkspace, IViewParent<T,P> where T : class
	{
		GraphWithRoots<T,P> graph;
		IGraphView<T,P> view;
		LayoutParams layoutParams;
        IGraphBackendInternal<T,P> graphBackend;
		DebugSettings debugSettings;
		Rect drawRect;
		IEnumerator layoutEnumerator;
		LayoutType layoutType;
		LayoutType defaultLayoutType = LayoutType.Graph;
		HashSet<T> rootEntities;
		TweenCollection graphPosTweens;
        bool hasGraphPosChanges;

		//layout
		Stopwatch layoutTimer = new Stopwatch();
		float nextVertexPosTweenUpdate;
		const float maxFrameDuration = 0.004f;	// seconds
		const float vertexPosTweenDuration = 0.4f; // seconds
		const float vertexPosTweenUpdateInterval = 0.25f; // seconds

        Action Repaint;
        Action<Action> Exec;

		static Dictionary<LayoutType, GUIContent> layoutButtonContent = new Dictionary<LayoutType, GUIContent>()
		{
			{ LayoutType.Graph, new GUIContent("Graph", "Use graph layout") },
			{ LayoutType.Tree, new GUIContent("Tree", "Use tree layout") }
		};

#if DEBUG
		// debug settings
		bool permaRepaint;	// repaint permanently
#endif

		internal Workspace(Type backendType, object[] targets, Func<RelationsInspectorAPI> GetAPI, Action Repaint, Action<Action> Exec)
		{
            this.Repaint = Repaint;
            this.Exec = Exec;
            this.graphBackend = (IGraphBackendInternal<T, P>) BackendUtil.CreateBackendDecorator(backendType); 

            // create new layout params, they are not comming from the cfg yet
            this.layoutParams = ScriptableObject.CreateInstance<LayoutParams>();
			this.debugSettings = ScriptableObject.CreateInstance<DebugSettings>();
			this.layoutType = (LayoutType) GUIUtil.GetPrefsInt(GetPrefsKeyLayout(), (int)defaultLayoutType);			
			graphPosTweens = new TweenCollection();

            rootEntities = graphBackend.Init(targets, GetAPI() ).ToHashSet();

            // when targets is null, show the toolbar only. don't create a graph (and view)
            // when rootEntities is empty, create graph and view anyway, so the user can add entities
            if (targets != null)
                InitGraph(targets);
		}

		string GetPrefsKeyLayout()
		{
			return System.IO.Path.Combine(graphBackend.GetType().ToString(), "LayoutType");
		}

		void InitGraph(object[] targets)
		{
			graph = GraphBuilder<T, P>.Build(rootEntities, graphBackend.GetRelated, graphBackend.GetRelating, int.MaxValue);
            if (graph == null)
                return;
            
            bool didLoadLayout = GraphPosSerialization.LoadGraphLayout(graph, graphBackend.GetDecoratedType());
            if (didLoadLayout)
            {
                // delay the view construction, so this.drawRect can be set before that runs.
                Exec( () => view = new IMView<T, P>(graph, this) );
            }
            else
                Exec(() => DoAutoLayout(true));
            				
		}

		public void Update()
		{
			UpdateLayout();
			graphPosTweens.Update();

			bool doRepaint = false;
            hasGraphPosChanges = graphPosTweens.HasChanges;
            doRepaint |= hasGraphPosChanges;
            doRepaint |= Tweener.gen.HasChanges;
#if DEBUG
			if (permaRepaint)
				doRepaint = true;
#endif
			if(doRepaint)
				Repaint();
		}

		void UpdateLayout()
		{
			if (layoutEnumerator == null)
				return;

			layoutTimer.Reset();
			layoutTimer.Start();
			bool generatorActive;
			do
			{
				generatorActive = layoutEnumerator.MoveNext();
			} while (generatorActive && (layoutTimer.ElapsedTicks/(float)Stopwatch.Frequency) < maxFrameDuration);

			var positions = layoutEnumerator.Current as Dictionary<T, Vector2>;
			bool gotFinalPositions = !generatorActive;
			if (!generatorActive)
				layoutEnumerator = null;

			var time = EditorApplication.timeSinceStartup;
			if (gotFinalPositions || time > nextVertexPosTweenUpdate)
			{
				nextVertexPosTweenUpdate = (float)time + vertexPosTweenUpdateInterval;
				if( positions != null)
				{
					foreach (var pair in positions)
					{
						VertexData<T, P> vData = null;
						if( graph.VerticesData.TryGetValue(pair.Key, out vData) )
							graphPosTweens.Add( new Tween<Vector2>( v=> vData.pos = v, vertexPosTweenDuration, TweenUtil.Vector2_2(vData.pos, pair.Value, TwoValueEasing.Linear)));
					}
				}
			}
		}

        public void Relayout()
        {
            Exec( () => DoAutoLayout( false ) );
        }

		public void OnGUI(Rect drawRect)
		{					
			this.drawRect = drawRect;

			if (view != null)
			{
				if (hasGraphPosChanges)
					view.FitViewRectToGraph();
				
				try
				{
					view.Draw();
				}
				catch (MissingReferenceException)
				{
					graph.CleanNullRefs();
				}

				if (graph != null && Settings.Instance.showMinimap)
				{
					var entityPositions = graph.VerticesData.Values.Select(data => data.pos);
					var style = SkinManager.GetSkin().minimap;
                    Rect minimapRect = Minimap.GetRect( SkinManager.GetSkin().minimap, Settings.Instance.minimapLocation, drawRect ); 
                    var newCenter = Minimap.Draw( entityPositions, minimapRect, view.GetViewRect(drawRect), debugSettings.showMinimapGraphBounds, style);
					view.SetCenter(newCenter);
				}

				view.HandleEvent(Event.current);
			}
		}

		public void OnToolbarGUI()
		{		
			EditorGUI.BeginChangeCheck();

            if (graph != null && graph.IsTree())
            {
                layoutType = (LayoutType)GUIUtil.EnumToolbar("", layoutType, (t) => layoutButtonContent[(LayoutType)t], EditorStyles.miniButton);
                if (EditorGUI.EndChangeCheck())
                {
                    GUIUtil.SetPrefsInt(GetPrefsKeyLayout(), (int)layoutType);
                    Exec(() => DoAutoLayout());
                }
            }
#if DEBUG
			// option to repaint constantly
			permaRepaint = GUILayout.Toggle( permaRepaint, "perma-repaint", GUILayout.ExpandWidth(false));

			// option to run the layout
			if (GUILayout.Button("Run layout", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				Exec( () => DoAutoLayout() );
			}

			/*
			EditorGUI.BeginDisabledGroup(layoutEnumerator == null);
			if (GUILayout.Button("step", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				if (!layoutEnumerator.MoveNext())
					layoutEnumerator = null;
			}
			EditorGUI.EndDisabledGroup();
			*/
			
			if (GUILayout.Button("Layout params", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
			{
				var window = EditorWindow.CreateInstance<InspectorWindow>();
				window.objectToInspect = layoutParams;
				window.Show();
			}

            if(GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GraphPosSerialization.SaveGraphLayout(graph, graphBackend.GetDecoratedType());
            }

            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                GraphPosSerialization.LoadGraphLayout(graph, graphBackend.GetDecoratedType());
            }

#endif
            if (view != null)
			{
				GUILayout.FlexibleSpace();
				view.OnToolbarGUI();
			}
		}

		public Rect OnControlsGUI()
		{
			if (graphBackend != null)
				return graphBackend.OnGUI( );

			return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
		}

		public void OnSelectionChange()
		{
            if (graphBackend != null)
                graphBackend.OnUnitySelectionChange();
		}

        public void OnEvent(Event e)
        {
            if (graphBackend != null)
                graphBackend.OnEvent(e);
        }

        public void OnDestroy()
        {
            if (graph != null)
                GraphPosSerialization.SaveGraphLayout(graph, graphBackend.GetDecoratedType());
        }

		void DoAutoLayout(bool firstTime = false )
		{
			if (graph == null)
				return;

			if (layoutParams == null)
				Debug.LogError("canvas auto-layout: missing params");
			else
			{
				layoutEnumerator = GraphLayout<T, P>.Run(graph,  firstTime, layoutType, layoutParams);
			}

			view = new IMView<T, P>(graph, this);
		}

		#region implementing IWorkspace

		void IWorkspace.AddEntity(object entity, Vector2 position)
		{
			var asT = entity as T;
			if (asT == null )
				return;

			if(graph != null)
				graph.AddVertex(asT, position);
		}

		void IWorkspace.RemoveEntity(object entityObj)
		{
			var entity = entityObj as T;
			if (entity == null)
				return;

			graph.RemoveVertex(entity);
			if (view != null)
				view.OnRemovedEntity(entity);
		}

		void IWorkspace.CreateEdge(object[] sourceEntities, object tagObj)
		{
			if (sourceEntities == null)
				throw new System.ArgumentException("sourceEntities");

			var tag = (P)tagObj;

			if (view != null)
				view.CreateEdge(sourceEntities.Select(obj => obj as T), tag);
		}

		void IWorkspace.AddEdge(object sourceObj, object targetObj, object tagObj)
		{
			if (graph == null)
				return;

			var source = sourceObj as T;
			var target = targetObj as T;
			
			P tag;
			try{
				tag = (P)tagObj;
			}catch(System.InvalidCastException){
				return;
			}

			if (source == null || target == null)
				return;

			var edge = new Edge<T, P>(source, target, tag);
			graph.AddEdge(edge);
		}

		void IWorkspace.RemoveEdge(object sourceObj, object targetObj, object tagObj)
		{
			if (graph == null) return;

			var source = sourceObj as T;
			var target = targetObj as T;

			P tag;
			try
			{
				tag = (P)tagObj;
			}
			catch (System.InvalidCastException)
			{
				return;
			}

			if (source == null || target == null)
				return;

			var sourceOutEdges = graph.GetOutEdges(source);
			if (sourceOutEdges == null)
			{
				return;
			}
			var matchingEdges = sourceOutEdges.Where(e => e.Matches(source, target, tag));
			var edge = matchingEdges.FirstOrDefault();
			if (edge == null)
				return;

			graph.RemoveEdge(edge);
		}

		void IWorkspace.SelectEntityNodes(System.Predicate<object> doSelect)
		{
			view.SelectEntityNodes(doSelect);
		}

		#endregion

		#region implemention IViewParent

		public void RepaintView()
		{
			Repaint();
		}

		public Rect GetViewRect()
		{
			return drawRect;
		}

		public void MoveEntity(T entity, Vector2 delta)
		{
			Exec( () => MoveEntityCmd(entity, delta) );
		}

		void MoveEntityCmd(T entity, Vector2 delta)
		{
			Vector2 newPos = graph.GetPos(entity) + delta;
			graph.SetPos(entity, newPos);
		}

		public bool IsRoot(T entity)
		{
			return rootEntities.Contains(entity);
		}

		public IGraphBackendInternal<T, P> GetBackend()
		{
			return graphBackend;
		}

		#endregion
	}
}
