using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;


namespace RelationsInspector.Backend.SceneRefBackend
{
	using ObjGraph = Dictionary<Object, HashSet<Object>>;
	using ObjNodeGraph = Dictionary<SceneObjectNode, HashSet<SceneObjectNode>>;
	
	public static class ObjectDependencyUtil
	{
		public static ObjNodeGraph GetReferenceGraph(string sceneFilePath, HashSet<Object> targets)
		{
			// get the scene's objects
			var sceneObjects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(sceneFilePath);

			// get the scene gameObjects
			var sceneGOs = sceneObjects.Select(obj => obj as GameObject).Where(go => go != null);

			// get the root gameObjects
			var rootGOs = sceneGOs.Where(go => go.transform.parent == null);

			// build the Object graph
			var objGraph = new ObjGraph();
			foreach (var rootGO in rootGOs)
				AddToReferenceGraph(rootGO, null, objGraph, targets);

			// convert it to a SceneObjectNode graph, so we can destroy the objects
			string fileName = System.IO.Path.GetFileName(sceneFilePath);
			var nodeGraph = ObjectGraphToObjectNodeGraph(objGraph, obj => GetSceneObjectNodeName(obj, targets, fileName));

			// destroy the Objects
			for (int i = 0; i < sceneObjects.Length; i++)
				Object.DestroyImmediate(sceneObjects[i]);

			return nodeGraph;
		}


		// adds go and its children to the reference graph
		// expects none of them to already be vertices
		static void AddToReferenceGraph(GameObject go, GameObject parentGo, ObjGraph graph, IEnumerable<Object> targets)
		{
			// go may already be in the graph (a prefab can be referenced by more than one scene object)
			if (graph.ContainsKey(go))
			{
				if (parentGo != null)
				{
					graph[parentGo].ExceptWith(graph[go]);
					graph[parentGo].Add(go);
				}
				return;
			}

			// find all targets referenced by go
			var referencedObjects = EditorUtility.CollectDependencies( new Object[] { go } );
			var referencedTargets = referencedObjects.Intersect( targets );

			// only add go to the graph if it references any targets
			if (!referencedTargets.Any())
				return;

			graph[go] = new HashSet<Object>(referencedTargets);

			// remove the references from parent that it shares with go. 
			// they might both reference the same target, but we have no way of checking that.
			if (parentGo != null)
			{
				graph[parentGo].ExceptWith(graph[go]);
				graph[parentGo].Add(go);
			}

			// don't add intermediate nodes after a prefab
			if (IsPrefab(go))
				return;

			// if it's a prefab instance, add the prefab object
			var prefabParent = PrefabUtility.GetPrefabParent(go) as GameObject;
			if (prefabParent != null)
			{
				AddToReferenceGraph(prefabParent, go, graph, targets);
				return;
			}

			foreach (Transform child in go.transform)
				AddToReferenceGraph(child.gameObject, go, graph, targets);
		}

		// turn object graph into VisualNode graph (mapping obj -> name)
		static ObjNodeGraph ObjectGraphToObjectNodeGraph(ObjGraph objGraph, System.Func<Object, string> getObjectName)
		{
			// get all graph objects
			var allObjs = new HashSet<Object>(objGraph.Keys).Union(objGraph.Values.SelectMany(set => set));

			// map them to VisualNodes
			var objToNode = allObjs.ToDictionary(obj => obj, obj => new SceneObjectNode(getObjectName(obj)));

			// convert graph dictionary
			return objGraph.ToDictionary(pair => objToNode[pair.Key], pair => new HashSet<SceneObjectNode>(pair.Value.Select(obj => objToNode[obj])));
		}

		// get an object label. for target objects, we add the scene's name. for prefabs we add a postfix.
		static string GetSceneObjectNodeName(Object obj, HashSet<Object> targetObjs, string sceneName)
		{
			if (targetObjs.Contains(obj))
				return obj.name + "\nscene " + sceneName;

			if (IsPrefab(obj))//(obj == PrefabUtility.GetPrefabParent(obj))
				return obj.name + "\n(Prefab)";

			return obj.name;
		}

		// returns true if the object is a prefab
		static bool IsPrefab(Object obj)
		{
			return PrefabUtility.GetPrefabParent(obj) == null && PrefabUtility.GetPrefabObject(obj) != null;
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
