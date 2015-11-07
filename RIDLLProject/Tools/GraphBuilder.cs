using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public static class GraphBuilder<T,P> where T : class
	{
		public delegate IEnumerable<Relation<T,P>> GetRelations(T item);
        delegate void OnVertexAdded( T vertex, T related );

		public static GraphWithRoots<T,P> Build(IEnumerable<T> seeds, GetRelations getRelations, RNG rng, int maxNodeCount)
		{
			var graph = new GraphWithRoots<T, P>();
			
			// add the root element
			if ( seeds == null || !seeds.Any() )
				return graph;

			foreach(var root in seeds)
				graph.AddVertex(root, RndPos(rng, 5f) );

            AddRelations( graph, seeds, getRelations, rng, maxNodeCount);
			return graph;
		}

        static void AddRelations( Graph<T, P> graph, IEnumerable<T> entities, GetRelations getRelations, RNG rng, int maxNodeCount)
        {
            var unexploredEntities = new List<T>();
            foreach ( var entity in entities )
            {
                foreach ( var relation in getRelations( entity ) )
                {
                    if ( !IsValidFor( relation, entity ) )
                        continue;

                    if ( graph.ContainsEdge( relation ) )
                        continue;

                    if ( entity == relation.Source )
                        graph.genDownwards = true;
                    else
                        graph.genUpwards = true;
                    
                    var otherEntity = relation.Opposite( entity );

                    if ( graph.ContainsVertex( otherEntity ) )
                    {
                        graph.AddEdge( relation );
                    }
                    else
                    {
                        if ( graph.VertexCount < maxNodeCount )
                        {
                            var pos = graph.GetPos( entity ) + RndPos( rng, 1f );
                            if ( graph.AddVertex( otherEntity, pos ) )
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
                AddRelations( graph, unexploredEntities, getRelations, rng, maxNodeCount );
        }

        // returns tree if the given entity is part of the given relation, and if relation can be added to the graph
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

        static Vector2 RndPos( RNG rng, float range )
        {
            return new Vector2( rng.Range( -range / 2, range / 2 ), rng.Range( -range / 2, range / 2 ) );
        }

        // add relations involving the given entity (which is part of the given graph). stop when the graph size hits maxNodes
        internal static void Expand( Graph<T, P> graph, T entity, GetRelations getRelations, RNG rng, int maxNodes )
        {
            graph.VerticesData[ entity ].unexplored = false;
            AddRelations( graph, new[] { entity }, getRelations, rng, maxNodes );
        }

        internal static void Append( Graph<T, P> graph, IEnumerable<T> appendTargets, Vector2 position, GetRelations getRelations, RNG rng, int maxNodes )
        {
            var newTargets = appendTargets.Where( ent => !graph.ContainsVertex( ent ) ).ToArray();

            foreach ( var entity in newTargets )
                graph.AddVertex( entity, position + RndPos(rng,1f) );
            
            AddRelations( graph, newTargets, getRelations, rng, maxNodes );
        }
    }
}
