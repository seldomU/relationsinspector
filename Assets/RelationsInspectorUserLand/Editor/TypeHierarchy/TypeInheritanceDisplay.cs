using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class TypeInheritanceDisplay : ITypeInheritanceDisplay
	{
		RelationsInspectorAPI _riAPI;		
		RelationsInspectorAPI riAPI
		{
			get
			{
				if (_riAPI == null || _riAPI.Equals(null) )	// UnityEngine.Object overrides == operator
				{
                    // we can't just use GetWindow here. 
                    // After assembly reload, this code might run before the RI window gets deserialized
                    // GetWindow would instanciate a second one.
                    var window = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>().FirstOrDefault();
					if (window == null)
						window = EditorWindow.GetWindow<RelationsInspectorWindow>();
                    _riAPI = window.GetAPI();
				}
				return _riAPI;
			}
		}

		public void SetTargetType(Type newTargetType)
		{
			riAPI.SetBackend( typeof(TypeInheritanceBackend) );
			riAPI.ResetTargets( new[] { newTargetType } );
		}
	}
}
