using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class TypeInheritanceDisplay : ITypeInheritanceDisplay
	{
		RelationsInspectorAPI riAPI;	// = EditorWindow.GetWindow<RelationsInspectorWindow>() as RelationsInspectorAPI;

		public TypeInheritanceDisplay()
		{
			// we can't just use GetWindow here. 
			// After assembly reload, this code might run before the RI window gets deserialized
			// GetWindow would instanciate a second one.
			var window = Resources.FindObjectsOfTypeAll<EditorWindow>().Where(w => w is RelationsInspectorWindow).SingleOrDefault();
			if (window == null)
				window = EditorWindow.GetWindow<RelationsInspectorWindow>();
			riAPI = window as RelationsInspectorAPI;
		}

		public void SetTargetType(Type newTargetType)
		{
			riAPI.ResetTargets( new[] { newTargetType } );
		}
	}
}
