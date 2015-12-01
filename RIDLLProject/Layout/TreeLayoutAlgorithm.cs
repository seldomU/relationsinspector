using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
    internal enum TreeRootLocation { Top, Left, Bottom, Right };

    public class TreeLayoutAlgorithm<T,P> where T : class
	{
		Graph<T, P> graph;
		Dictionary<T, float> treeWidth;	// width of the tree under the entity, with each entity being one unit wide
		const float entityWidth = 1f;
		const float entityHeight = 1f;
		Vector2 RootPos = new Vector2(0, 0);

		public TreeLayoutAlgorithm(Graph<T, P> graph)
		{
			this.graph = graph;
			treeWidth = new Dictionary<T, float>();
		}

		public IEnumerator Compute()
		{
            var root = graph.RootVertices.SingleOrDefault();
			
			if (root == null)
				yield break;

			var positions = new Dictionary<T, Vector2>();
			positions[root] = RootPos;
			PositionChildren(root, positions);

            // "rotate" positions according to the desired root node location 
            var nodeTransform = GetNodePositionTransform( Settings.Instance.treeRootLocation );
            foreach ( var node in graph.Vertices )
                positions[ node ] = nodeTransform( positions[ node ] );

			yield return positions;
		}

		// set the positions of the entity's children (recursively), so that they are centered below entity
		void PositionChildren(T entity, Dictionary<T,Vector2> positions)
		{
			var children = graph.GetChildren(entity);
			var totalWidth = children.Sum( c => GetTreeWidth(c) );
			Vector2 entityPos = positions[entity];
			float xpos = entityPos.x - totalWidth / 2f;
			float height = entityPos.y + entityHeight;
			foreach (var child in children)
			{
				float childWidth = GetTreeWidth(child);
				positions[child] = new Vector2(xpos + childWidth / 2f, height);
				xpos += childWidth;
				PositionChildren(child, positions);
			}
		}

		// get the width of the tree under the root entity. this populates the treeWidth dict with root and all its children
		float GetTreeWidth(T root)
		{
			if (!treeWidth.ContainsKey(root))
			{
				var children = graph.GetChildren(root);
				float width = children.Any() ? children.Select(v => GetTreeWidth(v)).Sum() : entityWidth;
				treeWidth[root] = width;
			}
			return treeWidth[root];
		}

        // this becomes superfluous when the view supports rotation
        System.Func<Vector2,Vector2> GetNodePositionTransform( TreeRootLocation rootLocation )
        {
            switch ( rootLocation )
            {
                case TreeRootLocation.Top:
                default:
                    return pos => pos;  // identiy. keep position as it is

                case TreeRootLocation.Left:
                    return pos => new Vector2( pos.y, pos.x );  // flip x and y

                case TreeRootLocation.Bottom:
                    return pos => new Vector2( pos.x, -pos.y ); // mirror y 

                case TreeRootLocation.Right:
                    return pos => new Vector2( -pos.y, -pos.x ); // mirror both dimensions and flip
            }
        }
	}
}
