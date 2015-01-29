using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
	public static class GraphBuilder<T,P> where T : class
	{
		public delegate IEnumerable<Tuple<T,P>> GetRelations(T item);

		public static GraphWithRoots<T,P> Build(IEnumerable<T> roots, GetRelations GetRelations, int recursionSteps)
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
			var processQueue = new System.Collections.Generic.Queue<T>( roots );

			while (processQueue.Any())
			{
				T vertex = processQueue.Dequeue();
				recursionSteps--;

				foreach (var relation in GetRelations(vertex))
				{
					var target = relation._1;
					var tag = relation._2;

					if (target == null)
						continue;

					if (!graph.ContainsVertex(target))
					{
						graph.AddVertex(target);
						if( recursionSteps > 0 )
							processQueue.Enqueue(target);
					}

					graph.AddEdge( new Edge<T,P>(vertex, target, tag) );
				}
			}

			return graph;
		}
	}
}
