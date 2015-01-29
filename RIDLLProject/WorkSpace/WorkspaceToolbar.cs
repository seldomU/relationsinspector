using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RelationsInspector
{
	internal class WorkspaceToolbar
	{
		GUIStyle toolBarStyle { get { return EditorStyles.toolbar; } }
		GUIStyle buttonStyle { get { return EditorStyles.toolbarButton; } }
		GUIStyle dropDownStyle { get { return EditorStyles.toolbarDropDown; } }

		LayoutParams layoutParams;
		DebugSettings debugSettings;
		Action runLayout;			// call this to run the graph layout algorithm
		public bool selectionLock;

		internal WorkspaceToolbar(LayoutParams layoutParams, DebugSettings debugSettings, Action runLayout)
		{
			this.layoutParams = layoutParams;
			this.debugSettings = debugSettings;
			this.runLayout = runLayout;
		}

		internal void OnGUI()
		{
			EditorGUILayout.BeginHorizontal(toolBarStyle);

			DrawToolBarItems();

			EditorGUILayout.EndHorizontal();
		}

		// toolbar GUI code
		void DrawToolBarItems()
		{
			GUILayout.FlexibleSpace();
			selectionLock = GUILayout.Toggle(selectionLock, "Lock selection");
			ShowButton("Layout parameters", () => ShowInspector(layoutParams));
			ShowButton("Debug settings", () => ShowInspector(debugSettings));
			GUILayout.Space(5);
			ShowButton("Run layout", runLayout);
		}

		// spawn a inspector window for the layout params
		void ShowInspector(UnityEngine.Object toInspect)
		{
			var window = EditorWindow.CreateInstance<InspectorWindow>();
			window.objectToInspect = toInspect;
			window.Show();
		}

		// utility: bind a button to an action
		void ShowButton(string labelText, Action action)
		{
			if (GUILayout.Button(labelText, buttonStyle, GUILayout.ExpandWidth(false)))
				action.Invoke();
		}
	}
}