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
				AddToReferenceGraph(rootGO, objGraph, targets);

			// convert it to a SceneObjectNode graph, so we can destroy the objects
			string fileName = System.IO.Path.GetFileName(sceneFilePath);
			var nodeGraph = ConvertObjectToNodeGraph(objGraph, obj => GetSceneObjectNodeName(obj, targets, fileName));

			// destroy the Objects
			for (int i = 0; i < sceneObjects.Length; i++)
				Object.DestroyImmediate(sceneObjects[i]);

			return nodeGraph;
		}

		// adds go and its children to the reference graph
		// expects none of them to already be vertices
		static void AddToReferenceGraph(GameObject go, ObjGraph graph, IEnumerable<Object> targets)
		{
			var dependencies = EditorUtility.CollectDependencies( new Object[] { go } );
			var referencedTargets = dependencies.Intersect( targets );

			if (!referencedTargets.Any())
				return;

			graph[go] = new HashSet<Object>(referencedTargets);

			var parent = go.transform.parent;
			if (parent != null)
			{
				graph[parent.gameObject].ExceptWith(referencedTargets);
				graph[parent.gameObject].Add(go);
			}

			foreach (Transform child in go.transform)
				AddToReferenceGraph(child.gameObject, graph, targets);
		}

		// turn object graph into VisualNode graph (mapping obj -> name)
		static ObjNodeGraph ConvertObjectToNodeGraph(ObjGraph objGraph, System.Func<Object, string> getObjectName)
		{
			// get all graph objects
			var allObjs = new HashSet<Object>(objGraph.Keys).Union(objGraph.Values.SelectMany(set => set));

			// map them to VisualNodes
			var objToNode = allObjs.ToDictionary(obj => obj, obj => new SceneObjectNode(getObjectName(obj)));

			// convert graph dictionary
			return objGraph.ToDictionary(pair => objToNode[pair.Key], pair => new HashSet<SceneObjectNode>(pair.Value.Select(obj => objToNode[obj])));
		}

		// get an object label. for target objects, we add the scene's name
		static string GetSceneObjectNodeName(Object obj, HashSet<Object> targetObjs, string sceneName)
		{
			return targetObjs.Contains(obj) ? obj.name + "\nscene " + sceneName : obj.name;
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
