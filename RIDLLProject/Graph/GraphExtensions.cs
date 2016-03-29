using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	public static class GraphExtensions
	{
		public static IEnumerable<T> GetNeighbors<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			return graph.GetParents( vertex ).Concat( graph.GetChildren( vertex ) );
		}

		public static IEnumerable<T> GetNeighborsExceptSelf<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			return graph.GetParentsExceptSelf( vertex ).Concat( graph.GetChildrenExceptSelf( vertex ) );
		}

		public static IEnumerable<T> GetChildren<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			if ( !graph.Vertices.Contains( vertex ) )
				return Enumerable.Empty<T>();

			return graph.VerticesData[ vertex ].OutEdges.Get().Select( e => e.Target );
		}

		public static IEnumerable<T> GetChildrenExceptSelf<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			return graph.GetChildren( vertex ).Where( c => c != vertex );
		}

		public static IEnumerable<T> GetParents<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			if ( !graph.Vertices.Contains( vertex ) )
				return Enumerable.Empty<T>();

			return graph.VerticesData[ vertex ].InEdges.Get().Select( e => e.Source );
		}

		public static IEnumerable<T> GetParentsExceptSelf<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			return graph.GetParents( vertex ).Where( p => p != vertex );
		}

		public static IEnumerable<Relation<T, P>> GetEdges<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			return graph.GetInEdges( vertex ).Concat( graph.GetOutEdges( vertex ) );
		}

		public static IEnumerable<Relation<T, P>> GetOutEdges<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			if ( !graph.Vertices.Contains( vertex ) )
				return Enumerable.Empty<Relation<T, P>>();

			return graph.VerticesData[ vertex ].OutEdges.Get();
		}

		public static IEnumerable<Relation<T, P>> GetInEdges<T, P>( this Graph<T, P> graph, T vertex ) where T : class
		{
			if ( !graph.Vertices.Contains( vertex ) )
				return Enumerable.Empty<Relation<T, P>>();

			return graph.VerticesData[ vertex ].InEdges.Get();
		}


		public static bool IsMultipleTrees<T, P>( this Graph<T, P> graph ) where T : class
		{
			var roots = graph.RootVertices;

			// no roots -> guaranteed cycle
			if ( !roots.Any() )
				return false;

			var visited = new HashSet<T>();
			var unexplored = new HashSet<T>( roots );

			while ( unexplored.Any() )
			{
				var item = unexplored.First();
				unexplored.Remove( item );
				visited.Add( item );

				var successors = graph.GetChildrenExceptSelf( item );    // ignore self edges

				if ( successors.Any( v => visited.Contains( v ) || unexplored.Contains( v ) ) )
					return false;

				unexplored.UnionWith( successors );
			}

			// subgraphs that have no roots (and thus contain cycles) haven't been visited yet
			// if any exists, this is not a tree
			return graph.Vertices.All( v => visited.Contains( v ) );
		}
	}
}
