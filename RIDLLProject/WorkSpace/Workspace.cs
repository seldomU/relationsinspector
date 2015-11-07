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
        IGraphBackendInternal<T,P> graphBackend;
		Rect drawRect;
		LayoutType layoutType;
		LayoutType defaultLayoutType = LayoutType.Graph;
		HashSet<T> seedEntities; 
		TweenCollection graphPosTweens;
        bool hasGraphPosChanges;

        //layout
        IEnumerator layoutEnumerator;
        Stopwatch layoutTimer = new Stopwatch();
		float nextVertexPosTweenUpdate;
        bool firstLayoutRun;

        Action Repaint;
        Action<Action> Exec;
        RelationsInspectorAPI API;

		static Dictionary<LayoutType, GUIContent> layoutButtonContent = new Dictionary<LayoutType, GUIContent>()
		{
			{ LayoutType.Graph, new GUIContent("Graph", "Use graph layout") },
			{ LayoutType.Tree, new GUIContent("Tree", "Use tree layout") }
		};

		internal Workspace(Type backendType, object[] targets, RelationsInspectorAPI API, Action Repaint, Action<Action> Exec)
		{
            this.Repaint = Repaint;
            this.Exec = Exec;
            this.API = API;
            this.graphBackend = (IGraphBackendInternal<T, P>) BackendUtil.CreateBackendDecorator(backendType); 

            // create new layout params, they are not comming from the cfg yet
			this.layoutType = (LayoutType) GUIUtil.GetPrefsInt(GetPrefsKeyLayout(), (int)defaultLayoutType);			
			graphPosTweens = new TweenCollection();

            seedEntities = graphBackend.Init(targets, API ).ToHashSet();

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
            graph = GraphBuilder<T, P>.Build(seedEntities, graphBackend.GetRelations, Settings.Instance.maxGraphNodes);
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
            var parms = Settings.Instance.layoutParams;
			do
			{
				generatorActive = layoutEnumerator.MoveNext();
			} while (generatorActive && (layoutTimer.ElapsedTicks/(float)Stopwatch.Frequency) < parms.maxFrameDuration);

			var positions = layoutEnumerator.Current as Dictionary<T, Vector2>;
			bool gotFinalPositions = !generatorActive;
			if (!generatorActive)
				layoutEnumerator = null;

			var time = EditorApplication.timeSinceStartup;
			if (gotFinalPositions || time > nextVertexPosTweenUpdate)
			{
				nextVertexPosTweenUpdate = (float)time + parms.vertexPosTweenUpdateInterval;
				if( positions != null)
				{
					foreach (var pair in positions)
					{
						VertexData<T, P> vData = null;
                        if (graph.VerticesData.TryGetValue(pair.Key, out vData))
                        {
                            T tweenOwner = pair.Key;
                            var evalFunc = TweenUtil.GetCombinedEasing(tweenOwner, graphPosTweens, vData.pos, pair.Value);
                            graphPosTweens.Replace( tweenOwner, new Tween<Vector2>(v => vData.pos = v, parms.vertexPosTweenDuration, evalFunc) );
                        }
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
					view.FitViewRectToGraph(firstLayoutRun);
				
				try
				{
					view.Draw();
				}
				catch (MissingReferenceException)
				{
					graph.CleanNullRefs();
				}

				view.HandleEvent(Event.current);
			}
		}

		public void OnToolbarGUI()
		{		
			EditorGUI.BeginChangeCheck();

            // let user pick a layout type (iff tree layout is an option)
            if (graph != null && graph.IsTree())
            {
                layoutType = (LayoutType)GUIUtil.EnumToolbar("", layoutType, (t) => layoutButtonContent[(LayoutType)t], EditorStyles.miniButton);
                if (EditorGUI.EndChangeCheck())
                {
                    GUIUtil.SetPrefsInt(GetPrefsKeyLayout(), (int)layoutType);
                    Exec(() => DoAutoLayout(false));
                }
            }

            // draw view controls
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

			layoutEnumerator = GraphLayout<T, P>.Run(graph,  firstTime, layoutType, Settings.Instance.layoutParams);
            firstLayoutRun = firstTime;

            if (firstTime)
			    view = new IMView<T, P>(graph, this);
		}

        #region implementing IWorkspace

        void IWorkspace.AddTargets( object[] targetsToAdd, Vector2 pos )
        {
            if ( targetsToAdd == null )
                return;

            var asT = targetsToAdd.OfType<T>().ToArray();
            if ( !asT.Any() )
                return;

            // transform pos to graph space
            seedEntities.UnionWith( asT );
            var graphPos = (view == null) ? Vector2.zero : view.GetGraphPosition( pos );
            GraphBuilder<T, P>.Append( graph, asT, graphPos, graphBackend.GetRelations, graph.VertexCount + Settings.Instance.maxGraphNodes );
            Exec( () => DoAutoLayout( false ) );
        }

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

        void IWorkspace.ExpandEntity( object entityObj )
        {
            var entity = entityObj as T;
            if ( entity == null )
                return;
            GraphBuilder<T, P>.Expand( graph, entity, graphBackend.GetRelations, graph.VertexCount + Settings.Instance.maxGraphNodes );
            Exec( () => DoAutoLayout( false ) );
        }

        void IWorkspace.FoldEntity( object entityObj )
        {
            var entity = entityObj as T;
            if ( entity == null )
                return;

            FoldUtil.Fold( graph, entity, IsSeed );
        }

		void IWorkspace.CreateRelation(object[] sourceEntities, object tagObj)
		{
			if (sourceEntities == null)
				throw new System.ArgumentException("sourceEntities");

			var tag = (P)tagObj;

			if (view != null)
				view.CreateEdge(sourceEntities.Select(obj => obj as T), tag);
		}

		void IWorkspace.AddRelation(object sourceObj, object targetObj, object tagObj)
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

			var relation = new Relation<T, P>(source, target, tag);
			graph.AddEdge(relation);
		}

		void IWorkspace.RemoveRelation(object sourceObj, object targetObj, object tagObj)
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

		public bool IsSeed(T entity)
		{
			return seedEntities.Contains(entity);
		}

		public IGraphBackendInternal<T, P> GetBackend()
		{
			return graphBackend;
		}

        public RelationsInspectorAPI GetAPI()
        {
            return API;
        }

        #endregion
    }
}
