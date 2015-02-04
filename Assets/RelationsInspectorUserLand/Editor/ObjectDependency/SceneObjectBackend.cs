using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.ObjectDependency
{
	[RelationsInspector(typeof(Object))]
	public class SceneObjectBackend : MinimalBackend<SerializedObject, string>
	{
		public override IEnumerable<SerializedObject> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;

			if (targets == null || !targets.Any())
				yield break;

			string sceneAssetPath = GetSceneAssetPath(targets.First());
			if (sceneAssetPath != null)
			{
				foreach (var sceneGO in YAMLUtil.GetSceneGameObjects(sceneAssetPath))
					yield return sceneGO;
			}
		}

		public override IEnumerable<SerializedObject> GetRelatedEntities(SerializedObject entity)
		{
			var asGameObject = entity as SerializedGameObject;
			if (asGameObject != null)
			{
				foreach (var c in asGameObject.components)
					yield return c;
				// prefab?
				yield break;
			}

			var asTransform = entity as SerializedTransform;
			if (asTransform != null)
			{
				foreach (var child in asTransform.children)
					yield return child;

				yield return asTransform.gameObject;

				if (asTransform.parent != null)
					yield return asTransform.parent;

				yield break;
			}
		}

		string GetSceneAssetPath(object obj)
		{
			var asUnityObject = obj as Object;
			if (asUnityObject == null)
				return null;

			string assetPath = AssetDatabase.GetAssetPath(asUnityObject);
			if (Path.GetExtension(assetPath) != ".unity")
				return null;

			return assetPath;
		}
	}
}
