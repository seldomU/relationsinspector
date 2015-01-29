using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
	class GUIUtil
	{
		public static string GetPrefsKey(string localKey)
		{
			// create a name that should be a unique EditorPrefs key
			// by prefixing the project name (namespacing)
			return System.IO.Path.Combine(ProjectSettings.EditorPrefsProjectPrefix, localKey);
		}

		public static void SetPrefsInt(string localKey, int value)
		{
			EditorPrefs.SetInt(GetPrefsKey(localKey), value);
		}

		public static int GetPrefsInt(string localKey)
		{
			return EditorPrefs.GetInt( GetPrefsKey(localKey) );
		}

		public static int GetPrefsInt(string localKey, int defaultValue)
		{
			string prefsKey = GetPrefsKey(localKey);
			if (!EditorPrefs.HasKey(prefsKey))
				return defaultValue;

			return EditorPrefs.GetInt(prefsKey);
		}

		public static void SetPrefsString(string localKey, string value)
		{
			EditorPrefs.SetString(GetPrefsKey(localKey), value);
		}

		public static string GetPrefsString(string localKey)
		{
			return EditorPrefs.GetString(GetPrefsKey(localKey));
		}

		public static string GetPrefsString(string localKey, string defaultValue)
		{
			string prefsKey = GetPrefsKey(localKey);
			if (!EditorPrefs.HasKey(prefsKey))
				return defaultValue;

			return EditorPrefs.GetString(prefsKey);
		}

		public static void SetPrefsType(string localKey, Type value)
		{
			SetPrefsString(localKey, value.ToString());
		}
		
		public static Type GetPrefsType(string localKey)
		{
			string typeName = GetPrefsString(localKey);
			return ReflectionUtil.GetType(typeName);
		}

		/*public static T GetPrefsEnum<T>(string localKey) where T : System.Enum
		{
			return (T)(Enum)(object)GetPrefsInt(localKey);
		}

		public static T GetPrefsEnum<T>(string localKey, T defaultValue) where T : System.Enum
		{
			return (T)(Enum)(object)GetPrefsInt(localKey, (int)(object)defaultValue);
		}

		public static void SetPrefsEnum<T>(string localKey, T enumValue) where T : System.Enum
		{
			SetPrefsInt(localKey, (int)(object)enumValue);
		}*/

		/// Creates a toolbar that is filled from an Enum. (CC-BY-SA, from http://wiki.unity3d.com/index.php?title=EditorGUIExtension)
		public static Enum EnumToolbar(string prefixLabel, Enum selected, GUIStyle style, params GUILayoutOption[] options)
		{
			string[] toolbar = Enum.GetNames(selected.GetType());
			Array values = Enum.GetValues(selected.GetType());

			int selected_index = 0;
			while (selected_index < values.Length)
			{
				if (selected.ToString() == values.GetValue(selected_index).ToString())
					break;
				selected_index++;
			}

			GUILayout.BeginHorizontal();
			if (!string.IsNullOrEmpty(prefixLabel))
			{
				GUILayout.Label(prefixLabel);
				GUILayout.Space(10);
			}

			selected_index = GUILayout.Toolbar(selected_index, toolbar, style, options);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			return (Enum)values.GetValue(selected_index);
		}
	}
}
