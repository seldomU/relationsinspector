using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;


namespace RelationsInspector.Backend.ObjectDependency
{
	public class FileIdHelper
	{
		static PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

		public static int GetFileId(Object obj)
		{
			UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(obj);
			inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

			return serializedObject.FindProperty("m_LocalIdentfierInFile").intValue;
		}
	}
}
