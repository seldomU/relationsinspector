using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
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
			var root = graph.Vertices.Where(v => graph.IsRoot(v)).SingleOrDefault();
			
			if (root == null)
				yield break;

			var positions = new Dictionary<T, Vector2>();
			positions[root] = RootPos;
			PositionChildren(root, positions);
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
	}
}
