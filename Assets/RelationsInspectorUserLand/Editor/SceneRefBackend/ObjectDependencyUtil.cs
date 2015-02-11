using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


namespace RelationsInspector.Backend.SceneRefBackend
{
	public static class ObjectDependencyUtil
	{
		public static Dictionary<Object, HashSet<Object>> GetDependencyGraph(string sceneFilePath, IEnumerable<Object> targets)
		{
			// get the scene's objects
			var sceneObjects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(sceneFilePath);

			// get the scene gameObjects
			var sceneGOs = sceneObjects.Select(obj => obj as GameObject).Where(go => go != null);

			// get the root gameObjects
			var rootGOs = sceneGOs.Where(go => go.transform.parent == null);

			// build the graph and merge it with the others
			return ObjectDependencyUtil.GetDependencyGraph(targets, rootGOs);
		}
		
		//returns a graph of dependency relations, from the targets to the rootGO children that depend on them, up to the rootGOs
		static Dictionary<Object, HashSet<Object>> GetDependencyGraph(IEnumerable<Object> targets, IEnumerable<GameObject> rootGOs)
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
		static Dictionary<Object, HashSet<Object>> GetReferenceGraph(IEnumerable<Object> targets, IEnumerable<GameObject> rootGOs)
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

		// merge two graphs
		public static void AddGraph<T>(Dictionary<T, HashSet<T>> graph, Dictionary<T, HashSet<T>> addedGraph) where T : class
		{
			foreach (var pair in addedGraph)
			{
				if (!graph.ContainsKey(pair.Key))
					graph[pair.Key] = pair.Value;
				else
					graph[pair.Key].UnionWith(pair.Value);
			}
		}
	}
}
