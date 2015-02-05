using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class CodeEditor : EditorWindow
	{
		ITypeInheritanceDisplay display;

		void OnEnable()
		{
			GetDisplay();
		}

		void GetDisplay()
		{
			var displayTypes = GetType().Assembly.GetTypes().Where(t => typeof(ITypeInheritanceDisplay).IsAssignableFrom(t) && t.IsClass );

			if (!displayTypes.Any())
				return;

			Debug.Log(displayTypes.First().Name);

			var ctorInfo = displayTypes.First().GetConstructor( Type.EmptyTypes );
			if (ctorInfo == null)
				return;

			display = ctorInfo.Invoke( new object[] { } ) as ITypeInheritanceDisplay;
		}

		void OnGUI()
		{
			EditorGUILayout.HelpBox("I am a strong and independent unity extension, looking for a friend to help me visualize relations between things.", MessageType.None);
			//display-independent code
			// ...
			if (display != null)
			{
				if (GUILayout.Button("show EditorWindow subtypes"))
					display.SetTargetType( typeof(EditorWindow) );
			}
		}

		[MenuItem("Window/CodeEditor")]
		static void SpawnWindow()
		{
			GetWindow<CodeEditor>();
		}
	}
}
