using UnityEngine;
using System.Collections;
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

            //GetRelations x = ent => GetRelations1( ent ).Concat( GetRelations2( ent ) );
            AddRelations( graph, roots, getRelations, maxNodeCount );
			return graph;
		}

        static void AddRelations( Graph<T, P> graph, IEnumerable<T> entities, GetRelations getRelations, int maxNodeCount )
        {
            var unexploredEntities = new List<T>();
            foreach ( var ent in entities )
            {
                foreach ( var rel in getRelations( ent ) )
                {
                    if ( !IsValidFor( rel, ent ) )
                        break;
                    
                    var otherEnt = rel.Opposite( ent );

                    if ( graph.ContainsVertex( otherEnt ) )
                    {
                        graph.AddEdge( rel );
                    }
                    else
                    {
                        if ( graph.VertexCount < maxNodeCount )
                        {
                            if ( graph.AddVertex( otherEnt ) )
                            {
                                unexploredEntities.Add( otherEnt );
                                graph.AddEdge( rel );
                            }
                        }
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
