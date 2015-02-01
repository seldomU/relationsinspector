using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class SubTypesBackend : MinimalBackend<Type, string>
	{
		static Assembly[] assemblies = new[]
		{
			typeof(GameObject).Assembly,
			typeof(UnityEditor.Editor).Assembly,
			//GetAssemblyByName("Assembly-CSharp-Editor"),
			//GetAssemblyByName("Assembly-CSharp")
		};

		static IEnumerable<Type> allTypes = assemblies.SelectMany(asm => asm.GetExportedTypes());

		int nodeCount = 0;
		const int maxNodes = 50;


		public override IEnumerable<Type> GetRelatedEntities(Type entity)
		{
			foreach (var type in GetSubtypes(entity))
			{
				if (nodeCount >= maxNodes)
					yield break;

				yield return type;
				nodeCount++;
			}
		}

		private IEnumerable<Type> GetSubtypes(Type inspectedType)
		{
			if (inspectedType.IsInterface)
			{
				// todo: generic interfaces
				var implementers = allTypes.Where(t => inspectedType.IsAssignableFrom(t));
				foreach (var type in implementers)
				{
					if (type.BaseType == null)
						yield return type;

					if (!type.BaseType.IsAssignableFrom(inspectedType))
						yield return type;

					// base type implements the interface, so this type is not directly related
				}
			}
			else
			{
				var subTypes = allTypes.Where( t => t.BaseType == inspectedType);
				foreach (var type in subTypes)
					yield return type;
			}
		}

		static Assembly GetAssemblyByName(string name)
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().
				   SingleOrDefault(assembly => assembly.GetName().Name == name);
		}

		public override void OnEntityContextClick(IEnumerable<Type> entities)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Show subtype graph"), false, () => api.ResetTargets(new[] { entities.First() }) );
			menu.ShowAsContext();
		}
	}
}
