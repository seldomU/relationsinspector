using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector
{
	internal class GraphLayout<T, P> where T : class
	{
		internal static IEnumerator Run(Graph<T, P> graph, LayoutType layoutType, LayoutParams layoutParams)
		{
			switch(layoutType)
			{
				case LayoutType.Graph:
					return RunGraphLayout(graph);
				case LayoutType.Tree:
				default:
					if (graph.IsTree(true))
						return RunTreeLayout(graph);
					else
						return RunGraphLayout(graph);
			}
		}

		internal static IEnumerator RunGraphLayout(Graph<T, P> graph )
		{
			var algorithm = new GraphLayoutAlgorithm<T, P>(graph);
			return algorithm.Compute();
		}

		internal static IEnumerator RunTreeLayout(Graph<T, P> graph)
		{
			var algorithm = new TreeLayoutAlgorithm<T, P>(graph);
			return algorithm.Compute();
		}
	}
}
