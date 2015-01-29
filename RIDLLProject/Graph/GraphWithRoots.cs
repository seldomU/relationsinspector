using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	public class GraphWithRoots<T, P> : Graph<T, P> where T : class
	{
		public HashSet<T> RootVertices { get; private set; }

		public GraphWithRoots() : base()
		{
			RootVertices = new HashSet<T>();
		}

		public GraphWithRoots(GraphWithRoots<T, P> source) : base(source)
		{
			RootVertices = new HashSet<T>( source.RootVertices);
		}

		public override bool AddVertex(T vertex)
		{
			bool gotAdded = base.AddVertex(vertex);
			if( gotAdded )
				RootVertices.Add(vertex);
			return gotAdded;
		}

		public override bool RemoveVertex(T vertex)
		{
			// get the outedge targets before vertex is removed
			var targets = this.GetChildren(vertex);

			bool gotRemoved = base.RemoveVertex(vertex);

			if (gotRemoved)
			{
				RootVertices.Remove(vertex);
				// add the targets that have no more in-edges
				RootVertices.UnionWith(targets.Where(v => IsRoot(v) ) );
			}

			return gotRemoved;
		}

		public override bool AddEdge(Edge<T, P> edge)
		{
			bool gotAdded = base.AddEdge(edge);
			if (gotAdded)
				RootVertices.Remove(edge.Target);

			return gotAdded;
		}

		public override bool RemoveEdge(Edge<T, P> edge)
		{
			bool gotRemoved = base.RemoveEdge(edge);
			if (gotRemoved)
				if (IsRoot(edge.Target))
					RootVertices.Add(edge.Target);

			return gotRemoved;
		}
	}
}
