using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend.SceneRefBackend
{
	
	[RelationsInspector(typeof(Object))]
	public class SceneRefBackend : MinimalBackend<Object, string>
	{
		Dictionary<Object, HashSet<Object>> dependencyGraph;
		string sceneDirPath = Application.dataPath;
		
		public override IEnumerable<Object> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			if (targets == null || !targets.Any())
				yield break;

			var targetObjs = BackendUtil.Convert<Object>(targets);
			if (!targetObjs.Any())
				yield break;

			// get all scene files
			var sceneFilePaths = Directory.GetFiles(sceneDirPath, "*.unity", SearchOption.AllDirectories);


			dependencyGraph = new Dictionary<Object, HashSet<Object>>();
			foreach (var path in sceneFilePaths)
			{
				// get the scene's objects
				var sceneObjects = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(path);

				// get the scene gameObjects
				var sceneGOs = sceneObjects.Select(obj => obj as GameObject).Where(go => go != null);

				// get the root gameObjects
				var rootGOs = sceneGOs.Where(go => go.transform.parent == null);

				// build the graph and merge it with the others
				AddGraph(dependencyGraph, ObjectDependencyUtil.GetDependencyGraph(targetObjs, rootGOs) );
			}
			//timer.Stop();
			//UnityEngine.Debug.Log(" ms " + timer.ElapsedMilliseconds);

			foreach (var o in targetObjs)
				yield return o;
		}

		public override IEnumerable<Object> GetRelatedEntities(Object entity)
		{
			if (dependencyGraph.ContainsKey(entity))
				return dependencyGraph[entity];

			UnityEngine.Debug.Log("unreferenced: " + entity );
			return Enumerable.Empty<Object>();
		}

		static void AddGraph(Dictionary<Object, HashSet<Object>> graph, Dictionary<Object, HashSet<Object>> addedGraph)
		{
			foreach (var pair in addedGraph)
			{
				if (!graph.ContainsKey(pair.Key) )
					graph[pair.Key] = pair.Value;
				else
					graph[pair.Key].UnionWith(pair.Value);
			}
		}
	}
}
