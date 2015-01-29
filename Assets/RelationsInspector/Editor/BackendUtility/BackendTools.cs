using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector.Backend
{
	public static class BackendTools
	{
		// returns the asset object's full directory path, or null
		public static string GetAssetDirectory(Object assetObj)
		{
			if (assetObj == null)
				return null;

			string path = AssetDatabase.GetAssetPath(assetObj);
			if (string.IsNullOrEmpty(path))
				return null;

			string fullPath = System.IO.Path.GetFullPath(path).Replace(@"\", @"/");
			return System.IO.Path.GetDirectoryName(fullPath);
		}

		// returns true if the given path is a valid asset directory
		public static bool IsValidAssetDirectory(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			if (!System.IO.Directory.Exists(path))
				return false;

			// has to be inside assets root directory
			if (!path.StartsWith(Application.dataPath))
				return false;

			return true;
		}

		// pair each item in the collection with the default value of type P
		public static IEnumerable<Tuple<T, P>> PairWithDefaultTag<T, P>(IEnumerable<T> collection)
		{
			return collection.Select(item => new Tuple<T, P>(item, default(P)));
		}

		public static GUIContent GetObjContent(Object obj)
		{
			GUIContent content = EditorGUIUtility.ObjectContent(obj, obj.GetType() );
			content.tooltip = content.text;
			return content;
		}

		public static Rect GetMaxRect()
		{
			return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
		}

		// create an asset of type T, store it at path and return the object
		public static T CreateAssetOfType<T>(string path) where T : ScriptableObject
		{
			var scriptableObject = ScriptableObject.CreateInstance<T>();
			path = AssetDatabase.GenerateUniqueAssetPath(path);

			AssetDatabase.CreateAsset(scriptableObject, path);
			AssetDatabase.SaveAssets();
			return scriptableObject;
		}

		// convert object collection to collection of type A, skipping the items that aren't A
		public static IEnumerable<A> Convert<A>(IEnumerable<object> collection) where A : class
		{
			if (collection == null)
				return Enumerable.Empty<A>();

			return collection.Select(item => item as A).Where(asA => asA != null);
		}
	}
}
