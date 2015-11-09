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
        RNG builderRNG;

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
            builderRNG = new RNG( 4 ); // chosen by fair dice role. guaranteed to be random.

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
            graph = GraphBuilder<T, P>.Build(seedEntities, graphBackend.GetRelations, builderRNG, Settings.Instance.maxGraphNodes);
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
            var parms = Settings.Instance.layoutTweenParameters;
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

            if ( graph != null && GUILayout.Button( "Relayout", EditorStyles.miniButton, GUILayout.ExpandWidth( false ) ) )
                Exec( () => API.Relayout() );

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
            if ( graphBackend != null && 
                e != null && 
                e.type == EventType.ExecuteCommand &&
                !string.IsNullOrEmpty(e.commandName) )
            {
                graphBackend.OnCommand( e.commandName );
                e.Use();
            }
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

			layoutEnumerator = GraphLayout<T, P>.Run(graph, layoutType, Settings.Instance.layoutTweenParameters);
            firstLayoutRun = firstTime;

            if (firstTime)
			    view = new IMView<T, P>(graph, this);
		}

        #region implementing IWorkspace

        void IWorkspace.AddTargets( object[] targetsToAdd, Vector2 pos )
        {
            if ( targetsToAdd == null )
            {
                Log.Error( "Can't add null targets" );
                return;
            }

            var asT = targetsToAdd.OfType<T>().ToArray();
            if ( asT.Count() != targetsToAdd.Count() )
            {
                Log.Error( "Can't add targets: not all entities are of type " + typeof( T ) );
                return;
            }

            if ( graph == null )
                return;

            seedEntities.UnionWith( asT );

            // when no position is given, use the top-left corner of the graph bounds
            if ( pos == Vector2.zero )
                pos = Util.GetBounds( graph.VerticesData.Values.Select( v => v.pos ) ).GetOrigin();
            else // transform pos to graph space
                pos = (view == null) ? Vector2.zero : view.GetGraphPosition( pos );

            GraphBuilder<T, P>.Append( graph, asT, pos, graphBackend.GetRelations, builderRNG, graph.VertexCount + Settings.Instance.maxGraphNodes );
            Exec( () => DoAutoLayout( false ) );
        }

		void IWorkspace.AddEntity(object entity, Vector2 position)
		{
			var asT = entity as T;
            if ( asT == null )
            {
                Log.Error( "Can't add entity: it is not of type " + typeof( T ) );
                return;
            }

            if ( graph == null )
                return;

            if ( !graph.AddVertex( asT, position ) )
                Log.Error( "Can't add entity to graph" );
		}

		void IWorkspace.RemoveEntity(object entityObj)
		{
			var entity = entityObj as T;
            if ( entity == null )
            {
                Log.Error( "Can't remove entity: it is not of type " + typeof( T ) );
                return;
            }

            if ( graph == null )
                return;

            if ( !graph.RemoveVertex( entity ) )
            {
                Log.Error( "Can't remove entity: it is not part of the graph." );
                return;
            }

			if (view != null)
				view.OnRemovedEntity(entity);
		}

        void IWorkspace.ExpandEntity( object entityObj )
        {
            var entity = entityObj as T;
            if ( entity == null )
            {
                Log.Error( "Can't expand entity: it is not of type " + typeof( T ) );
                return;
            }

            if ( graph == null )
                return;

            GraphBuilder<T, P>.Expand( graph, entity, graphBackend.GetRelations, builderRNG, graph.VertexCount + Settings.Instance.maxGraphNodes );
            Exec( () => DoAutoLayout( false ) );
        }

        void IWorkspace.FoldEntity( object entityObj )
        {
            var entity = entityObj as T;
            if ( entity == null )
            {
                Log.Error( "Can't fold entity: it is not part of the graph." );
                return;
            }

            if(graph != null )
                FoldUtil.Fold( graph, entity, IsSeed );
        }

		void IWorkspace.CreateRelation(object[] sourceEntities, object tagObj)
		{
            // validate source entities
            if ( sourceEntities == null )
            {
                Log.Error( "Can't create relations: entities are missing." );
                return;
            }

            var asT = sourceEntities.OfType<T>();
            if ( asT.Count() != sourceEntities.Count() )
            {
                Log.Error("Can't create relations: not all entities are of type " + typeof( T ) );
                return;
            }

            // validate tag
            if ( tagObj == null )
            {
                Log.Error( "Can't create relations: tag is null" );
                return;
            }

            P tag;
            try
            {
                tag = (P) tagObj;
            }
            catch ( InvalidCastException )
            {
                Log.Error( string.Format( "Can't create relations: tag is of type {0}, expected {1}", tagObj.GetType(), typeof( P ) ) );
                return;
            }

            if ( view != null )
                view.CreateEdge(asT, tag);
		}

		void IWorkspace.AddRelation(object sourceObj, object targetObj, object tagObj)
		{
            // validate source
			var source = sourceObj as T;
            if ( source == null )
            {
                Log.Error( "Can't add relation: source entity is not of type " + typeof( T ) );
                return;
            }

            // validate target
			var target = targetObj as T;
            if ( target == null )
            {
                Log.Error( "Can't add relation: target entity is not of type " + typeof( T ) );
                return;
            }

            // validate tag
            if ( tagObj == null )
            {
                Log.Error( "Can't add relations: tag is null" );
                return;
            }

            P tag;
			try
            {
				tag = (P)tagObj;
			}
            catch (InvalidCastException)
            {
                Log.Error(string.Format( "Can't add relation. Tag is of type {0}, expected {1}", tagObj.GetType(), typeof( P ) ) );
                return;
			}

            if ( graph != null )
			    graph.AddEdge( new Relation<T, P>( source, target, tag ) );
		}

		void IWorkspace.RemoveRelation(object sourceObj, object targetObj, object tagObj)
		{
            // validate source
            var source = sourceObj as T;
            if ( source == null )
            {
                Log.Error( "Can't remove relation: source entity is not of type " + typeof( T ) );
                return;
            }

            // validate target
            var target = targetObj as T;
            if ( target == null )
            {
                Log.Error( "Can't remove relation: target entity is not of type " + typeof( T ) );
                return;
            }

            // validate tag
            if ( tagObj == null )
            {
                Log.Error( "Can't remove relation: tag is null" );
                return;
            }

            P tag;
            try
            {
                tag = (P) tagObj;
            }
            catch ( InvalidCastException )
            {
                Log.Error( string.Format( "Can't add relation. Tag is of type {0}, expected {1}", tagObj.GetType(), typeof( P ) ) );
                return;
            }

            if ( graph == null )
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
            if(view != null )
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
