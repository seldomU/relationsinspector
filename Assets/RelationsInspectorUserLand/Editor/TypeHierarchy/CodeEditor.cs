using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;
using System.Collections.Generic;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class CodeEditor : EditorWindow
	{
		ITypeInheritanceDisplay display;

		void OnEnable()
		{
			// find and instanciate a type that implements ITypeInheritanceDisplay
			var displayTypes = GetType().Assembly.GetTypes().Where(t => typeof(ITypeInheritanceDisplay).IsAssignableFrom(t) && t.IsClass );
			if (displayTypes.Any())
				display = (ITypeInheritanceDisplay) System.Activator.CreateInstance( displayTypes.First() );
		}

		void OnGUI()
		{
			//display-independent code
			// ...
			if (display != null)
			{
				/*
				if (GUILayout.Button("show EditorWindow subtypes"))
					display.SetTargetType( typeof(EditorWindow) );

				if (GUILayout.Button("show List<> subtypes"))
					display.SetTargetType(typeof(List<>));
				*/
				if (GUILayout.Button("inspect XmlCharacterData"))
					display.SetTargetType(typeof(System.Xml.XmlCharacterData));
			}
		}

		[MenuItem("Window/CodeEditor")]
		static void SpawnWindow()
		{
			GetWindow<CodeEditor>();
		}
	}
}
