using UnityEngine;
using UnityEditor;
using System.Collections;

public class InspectorWindow : EditorWindow
{
	public Object objectToInspect;
	Editor objectEditor;
	Vector2 scrollPos;
	
	void OnGUI()
	{
		if (objectToInspect == null)
			return;

		if (objectEditor == null)
			objectEditor = Editor.CreateEditor(objectToInspect);

		scrollPos = EditorGUILayout.BeginScrollView( scrollPos );
		
		objectEditor.OnInspectorGUI();
		
		EditorGUILayout.EndScrollView();
	}
}
