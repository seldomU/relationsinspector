using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector
{
	internal class GraphLayout<T, P> where T : class
	{
		internal static IEnumerator Run(Graph<T, P> graph, bool firstTime, LayoutType layoutType, LayoutParams layoutParams)
		{
			switch(layoutType)
			{
				case LayoutType.Graph:
                    return RunGraphLayout(graph, firstTime);
				case LayoutType.Tree:
				default:
					if (graph.IsTree(true))
						return RunTreeLayout(graph);
					else
                        return RunGraphLayout(graph, firstTime);
			}
		}

        internal static IEnumerator RunGraphLayout(Graph<T, P> graph, bool rndInitPos)
		{
			var algorithm = new GraphLayoutAlgorithm<T, P>(graph);
			return algorithm.Compute(rndInitPos);
		}

		internal static IEnumerator RunTreeLayout(Graph<T, P> graph)
		{
			var algorithm = new TreeLayoutAlgorithm<T, P>(graph);
			return algorithm.Compute();
		}
	}
}
