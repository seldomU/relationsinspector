using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


namespace RelationsInspector.Backend.SceneRefBackend
{
	public static class ObjectDependencyUtil
	{
		//returns a graph of dependency relations, from the targets to the rootGO children that depend on them, up to the rootGOs
		public static Dictionary<Object, HashSet<Object>> GetDependencyGraph(IEnumerable<Object> targets, IEnumerable<GameObject> rootGOs)
		{
			var refGraph = GetReferenceGraph(targets, rootGOs);

			// dependency graph is the inverted reference graph
			var dependencyGraph = new Dictionary<Object, HashSet<Object>>();
			foreach (var pair in refGraph)
			{
				foreach (var item in pair.Value)
				{
					if (!dependencyGraph.ContainsKey(item))
						dependencyGraph[item] = new HashSet<Object>();
					dependencyGraph[item].Add(pair.Key);
				}
			}

			return dependencyGraph;
		}

		// returns a graph of the reference links down the gameObject hierarchy, from rootGOs down to targets
		public static Dictionary<Object, HashSet<Object>> GetReferenceGraph(IEnumerable<Object> targets, IEnumerable<GameObject> rootGOs)
		{
			var refGraph = new Dictionary<Object, HashSet<Object>>();

			foreach(var rootGO in rootGOs)
				refGraph[rootGO] = new HashSet<Object>( GetReferencedObjects(rootGO).Intersect(targets) );

			var unresolved = new Queue<GameObject>(rootGOs);

			while (unresolved.Any())
			{
				var go = unresolved.Dequeue();
				var goChildren = GetChildren(go).ToArray();
				foreach (var child in goChildren)
				{
					// detatch from parent
					child.transform.parent = null;

					// add map entry for child
					refGraph[child] = new HashSet<Object>( GetReferencedObjects(child).Intersect(targets) );

					// remove child targets from parent
					refGraph[go].ExceptWith( refGraph[child] );

					// add parent->child reference
					refGraph[go].Add(child);

					//re-attach to parent
					child.transform.parent = go.transform;
					unresolved.Enqueue( child );
				}
			}

			return refGraph;
		}

		static IEnumerable<GameObject> GetChildren(GameObject go)
		{
			foreach (Transform t in go.transform)
				yield return t.gameObject;
		}

		// returns those targets that the candidate references
		static IEnumerable<Object> GetReferencedObjects(Object obj)
		{
			return EditorUtility.CollectDependencies(new[] { obj });
		}
	}
}
