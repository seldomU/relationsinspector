using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.Linq;
using System.IO;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.ObjectDependency
{
	[RelationsInspector(typeof(Object))]
	public class ObjectDependencyBackend : MinimalBackend<SerializedObject, string>
	{
		List<SceneObjectGraph> sceneGraphs;
		
		public override IEnumerable<SerializedObject> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			sceneGraphs = ReadScenes();

			if (targets == null)
				return Enumerable.Empty<SerializedObject>();
			else
				return targets.SelectMany( t => GetReferencingSceneObjects(t as Object) );		
		}

		List<SceneObjectGraph> ReadScenes()
		{
			string sceneDirPath = Application.dataPath;
			string[] scenePaths = Directory.GetFiles(sceneDirPath, "*.unity", SearchOption.AllDirectories);
			return scenePaths.Select( path => new SceneObjectGraph(path) ).ToList();		
		}

		public override IEnumerable<SerializedObject> GetRelatedEntities(SerializedObject entity)
		{
			var owner = entity.owner;
			if (owner != null)
				yield return owner;
			yield break;
		}

		public override GUIContent GetContent(SerializedObject entity)
		{
			return new GUIContent( entity.name, YamlClassIdDictionary.GetClassName(entity.classId) );
		}

		IEnumerable<SerializedObject> GetReferencingSceneObjects(Object obj)
		{
			// Object: find scene objects that reference it
			if (obj == null)
				yield break;

			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
			string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
			//var objectId = YAMLUtil.GetObjectId(asObject);

			foreach (var sceneGraph in sceneGraphs)
			{
				foreach (var sceneObject in sceneGraph.objects)
				{
					if (sceneObject.ContainsGUIDRef(guid))
						yield return sceneObject;
				}
			}
		}
	}

	public class SceneObjectGraph
	{
		public List<SerializedObject> objects { get; private set; }

		public SceneObjectGraph(string path)
		{
			objects = YAMLUtil.GetSceneObjects(path).ToList();
		}
	}
}
