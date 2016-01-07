using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationsInspector
{
	class SubGraph<T>
	{
		public List<T> entryPoints = new List<T>();
		public HashSet<T> elements = new HashSet<T>();
		public int numSeeds;
		public SubGraph() { }
	}

	class FoldUtil
	{
		public static List<SubGraph<T>> Split<T, P>( Graph<T, P> graph, T root, Func<T, bool> isGraphSeed ) where T : class
		{
			var rootNeighbors = graph.GetNeighborsExceptSelf( root );
			var subGraphs = new List<SubGraph<T>>();
			foreach ( var seed in rootNeighbors )
			{
				var containingSubGraphs = subGraphs.Where( sg => sg.elements.Contains( seed ) );
				int numSG = containingSubGraphs.Count();
				if ( numSG > 1 )
				{
					throw new Exception( "found vertex in more than one subgraph" );
				}
				else if ( numSG == 1 )
				{
					containingSubGraphs.Single().entryPoints.Add( seed );
					continue;
				}
				else
					subGraphs.Add( FillSubGraph( seed, graph, root, isGraphSeed ) );
			}
			return subGraphs;
		}

		private static SubGraph<T> FillSubGraph<T, P>( T seed, Graph<T, P> graph, T root, Func<T, bool> isGraphSeed ) where T : class
		{

			var subGraph = new SubGraph<T>();
			subGraph.entryPoints.Add( seed );

			var neighbors = new HashSet<T>() { seed };

			while ( neighbors.Any() )
			{
				var item = neighbors.First();
				neighbors.Remove( item );
				if ( subGraph.elements.Contains( item ) )
					continue;

				subGraph.elements.Add( item );
				subGraph.numSeeds += isGraphSeed( item ) ? 1 : 0;
				neighbors.UnionWith( graph.GetNeighbors( item ).Except( new[] { root } ) );
			}
			return subGraph;
		}

		public static IEnumerable<T> GetFoldVertices<T, P>( Graph<T, P> graph, T foldEntity, Func<T, bool> isGraphSeed ) where T : class
		{
			var subGraphs = Split( graph, foldEntity, isGraphSeed );
			if ( !subGraphs.Any( sub => sub.numSeeds > 0 ) )
				return Enumerable.Empty<T>();

			return subGraphs
				.Where( sub => sub.numSeeds == 0 )
				.Where( sub => sub.entryPoints.Any( entry => graph.CanRegenerate( foldEntity, entry ) ) )
				.SelectMany( sub => sub.elements );
		}

		public static void Fold<T, P>( Graph<T, P> graph, T foldEntity, Func<T, bool> isGraphSeed ) where T : class
		{
			var removeEntities = GetFoldVertices( graph, foldEntity, isGraphSeed );

			if ( removeEntities.Any() )
				graph.VerticesData[ foldEntity ].unexplored = true;

			foreach ( var ent in removeEntities )
				graph.RemoveVertex( ent );
		}
	}
}
