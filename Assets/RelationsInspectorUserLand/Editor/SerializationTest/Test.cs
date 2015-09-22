using UnityEngine;
using System.Collections;
using UnityEditorInternal;
using RelationsInspector.Backend;
using System.Collections.Generic;
using RelationsInspector;
using System.IO;
using UnityEditor;
using System.Linq;

public class TestBackend : MinimalBackend<Object,string>
{
	public override IEnumerable<Object> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
	{
		if (targets == null || !targets.Any())
			yield break;

		var sceneFilePaths = targets.Select(obj => GetSceneAssetPath(obj)).Where(path => path != null);
		
		foreach (var path in sceneFilePaths)
		{
			var sceneObjects = InternalEditorUtility.LoadSerializedFileAndForget(path);
			Debug.Log(path + " -> " + sceneObjects.Length);
		}
		yield break;// return targets.First() as Object;
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
