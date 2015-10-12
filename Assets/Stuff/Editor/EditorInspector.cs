using UnityEditor;
using UnityEngine;
using System.Linq;

public class EditorInspector : EditorWindow
{
	int selectedWindowId;
	Vector2 inspectorScrollPos;

	void OnGUI()
	{
		// get the windows
		var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

		// pick one
		EditorGUILayout.Space();
		selectedWindowId = EditorGUILayout.Popup("Inspect window", selectedWindowId, windows.Select(w => w.title).ToArray());

		// get an inspector for it
		var inspectorEditor = Editor.CreateEditor(windows[selectedWindowId]);

		// draw the inspector
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Inspector " + windows[ selectedWindowId ].GetType(), EditorStyles.boldLabel);
		inspectorScrollPos = GUILayout.BeginScrollView(inspectorScrollPos);
		inspectorEditor.OnInspectorGUI();
		GUILayout.EndScrollView();
	}

	[MenuItem("Window/Editor inspector")]
	static void ShowWindow() { GetWindow<EditorInspector>(); }
}

