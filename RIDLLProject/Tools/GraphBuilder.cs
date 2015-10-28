using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
	public static class GraphBuilder<T,P> where T : class
	{
		public delegate IEnumerable<Relation<T,P>> GetRelations(T item);


		public static GraphWithRoots<T,P> Build(IEnumerable<T> roots, GetRelations getRelations, int maxNodeCount)
		{
			var graph = new GraphWithRoots<T, P>();
			
			// add the root element
			if ( roots == null || !roots.Any() )
				return graph;

			foreach(var root in roots)
				graph.AddVertex(root);

            AddRelations( graph, roots, getRelations, maxNodeCount);
			return graph;
		}

        static void AddRelations( Graph<T, P> graph, IEnumerable<T> entities, GetRelations getRelations, int maxNodeCount)
        {
            var unexploredEntities = new List<T>();
            foreach ( var entity in entities )
            {
                foreach ( var relation in getRelations( entity ) )
                {
                    if ( !IsValidFor( relation, entity ) )
                        break;
                    
                    var otherEntity = relation.Opposite( entity );

                    if ( graph.ContainsVertex( otherEntity ) )
                    {
                        graph.AddEdge( relation );
                    }
                    else
                    {
                        if ( graph.VertexCount < maxNodeCount )
                        {
                            if ( graph.AddVertex( otherEntity ) )
                            {
                                unexploredEntities.Add( otherEntity );
                                graph.AddEdge( relation );
                            }
                        }
                        else
                            graph.VerticesData[ entity ].unexplored = true;
                    }
                }
            }

            if( unexploredEntities.Any() )
                AddRelations( graph, unexploredEntities, getRelations, maxNodeCount );
        }

        // returns tree if the given relation contains entity and can be added to the graph
        static bool IsValidFor( Relation<T, P> relation, T entity )
        {
            if ( relation == null )
                return false;
            if ( relation.Source != entity && relation.Target != entity )
                return false;
            if ( Util.IsBadRef( relation.Source ) || Util.IsBadRef( relation.Target ) )
                return false;
            return true;
        }
	}
}
