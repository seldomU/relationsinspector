using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace RelationsInspector.Backend.TypeHierarchy
{

	public class TypeInheritenceBackend : MinimalBackend<Type, string>
	{
		bool includeSuperTypes;
		bool includeSubTypes;

		int nodeCount = 0;	// number of graph nodes
		int maxNodes = 60;	// upper limit of graph nodes

		static Assembly[] assemblies = new[]
		{
			typeof(GameObject).Assembly,
			typeof(UnityEditor.Editor).Assembly,
			TypeUtility.GetAssemblyByName("Assembly-CSharp-Editor"),
			TypeUtility.GetAssemblyByName("Assembly-CSharp")
		};


		public override IEnumerable<Type> GetRelatedEntities(Type entity)
		{
			var relatedTypes = Enumerable.Empty<Type>();
			
			if (includeSubTypes)
				relatedTypes = relatedTypes.Concat(TypeUtility.GetSubtypes(entity, assemblies));

			if (includeSuperTypes)
				relatedTypes = relatedTypes.Concat(TypeUtility.GetBaseTypeAndInterfaces(entity));

			foreach (var type in relatedTypes)
			{
				if (nodeCount >= maxNodes)
					yield break;

				yield return type;
				nodeCount++;
			}
		}

		public override Rect OnGUI()
		{
			GUILayout.BeginHorizontal();
			{
				includeSuperTypes = GUILayout.Toggle(includeSuperTypes, "SuperTypes", EditorStyles.toggle );
				includeSubTypes = GUILayout.Toggle(includeSubTypes, "SubTypes");
				maxNodes = EditorGUILayout.IntField("max nodes", maxNodes);
			}
			GUILayout.EndHorizontal();
			return BackendUtil.GetMaxRect();
		}

		public override void OnEntityContextClick(IEnumerable<Type> entities)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("make new target"), false, () => api.ResetTargets(new[] { entities.First() }));
			menu.ShowAsContext();
		}
	}
}
