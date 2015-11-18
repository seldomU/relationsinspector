using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
    public class GraphWithRoots<T, P> : Graph<T, P> where T : class
	{
		public HashSet<T> RootVertices { get; private set; }
        private bool isTree;
        private bool recalcIsTree;

		public GraphWithRoots() : base()
		{
			RootVertices = new HashSet<T>();
		}

		public GraphWithRoots(GraphWithRoots<T, P> source) : base(source)
		{
			RootVertices = new HashSet<T>( source.RootVertices);
		}

        public bool IsTree()
        {
            if(recalcIsTree)
            {
                recalcIsTree = false;
                isTree = this.IsSingleTree();
            }

            return isTree;
        }

		public override void AddVertex(T vertex)
		{
			base.AddVertex(vertex);
            // adding the vertex may have fail
            if (ContainsVertex( vertex ) )
            {
                recalcIsTree = true;
                RootVertices.Add(vertex);
            }
		}

		public override void RemoveVertex(T vertex)
		{
			// get the outedge targets before vertex is removed
			var targets = this.GetChildren(vertex);

			base.RemoveVertex(vertex);
            // removing the vertex may have failed
			if ( !ContainsVertex(vertex) )
			{
                recalcIsTree = true;
                RootVertices.Remove(vertex);
				// add the targets that have no more in-edges
				RootVertices.UnionWith(targets.Where(v => IsRoot(v) ) );
			}
		}

		public override void AddEdge(Relation<T, P> relation)
		{
			base.AddEdge(relation);
            // adding the relation may have failed
            if (ContainsEdge(relation) )
            {
                recalcIsTree = true;
                RootVertices.Remove(relation.Target);
            }
		}

		public override void RemoveEdge(Relation<T, P> relation)
		{
			base.RemoveEdge(relation);
            // removing the relation may have failed
            if (!ContainsEdge( relation ) )
            {
                recalcIsTree = true;
                if (IsRoot(relation.Target))
                    RootVertices.Add(relation.Target);
            }
		}
	}
}
