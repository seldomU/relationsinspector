using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
	public static class GraphBuilder<T,P> where T : class
	{
		public delegate IEnumerable<Tuple<T,P>> GetRelations(T item);

		public static GraphWithRoots<T,P> Build(IEnumerable<T> roots, GetRelations GetRelated, GetRelations GetRelating, int recursionSteps)
		{
			var graph = new GraphWithRoots<T, P>();
			
			// add the root element
			if ( roots == null || !roots.Any() )
				return graph;

			foreach(var root in roots)
				graph.AddVertex(root);

			// process elements that are in the graph already
			// add their successors (that are not in the graph yet) 
			// and add an edge between the element and its successor
			var successorQueue = new System.Collections.Generic.Queue<T>( roots );
            int sucRecSteps = recursionSteps;
			while (successorQueue.Any())
			{
				T vertex = successorQueue.Dequeue();
                sucRecSteps--;

				foreach (var relation in GetRelated(vertex))
				{
					var successor = relation._1;
					var tag = relation._2;

					if ( Util.IsBadRef(successor) )
						continue;

					if (!graph.ContainsVertex(successor))
					{
						graph.AddVertex(successor);
                        if (sucRecSteps > 0)
							successorQueue.Enqueue(successor);
					}

					graph.AddEdge( new Edge<T,P>(vertex, successor, tag) );
				}
			}

            // add root predecessors (that are not in the graph yet) 
            // and add edges from predecessors to vertices
            var predecessorQueue = new System.Collections.Generic.Queue<T>( roots );
            int predRecSteps = recursionSteps;
            while (predecessorQueue.Any())
			{
                T vertex = predecessorQueue.Dequeue();
                predRecSteps--;

                foreach (var relation in GetRelating(vertex))
				{
					var predecessor = relation._1;
					var tag = relation._2;

					if ( Util.IsBadRef(predecessor) )
						continue;

					if (!graph.ContainsVertex(predecessor))
					{
						graph.AddVertex(predecessor);
                        if (predRecSteps > 0)
                            predecessorQueue.Enqueue(predecessor);
					}

                    graph.AddEdge(new Edge<T, P>(predecessor, vertex, tag));
				}
			}

			return graph;
		}
	}
}
