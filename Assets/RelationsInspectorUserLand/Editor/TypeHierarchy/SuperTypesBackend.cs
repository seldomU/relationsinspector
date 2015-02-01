using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public class SuperTypesBackend : MinimalBackend<Type,string>
	{
		public override IEnumerable<Type> GetRelatedEntities(Type entity)
		{
			if (entity.IsInterface)
				yield break;

			var baseType = entity.BaseType;
			IEnumerable<Type> interfaces = entity.GetInterfaces();
			if (baseType != null)
			{
				yield return baseType;
				interfaces = interfaces.Except( baseType.GetInterfaces() );
			}

			foreach(var interfaceType in interfaces)
				yield return interfaceType;
		}

		public override void OnEntityContextClick(IEnumerable<Type> entities)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Show subtype graph"), false, () => ShowSubtypeGraph(entities.First()));
			menu.ShowAsContext();
		}

		void ShowSubtypeGraph(Type type)
		{
			api.SetBackend(typeof(SubTypesBackend));
			api.ResetTargets(new[] { type });
		}
	}
}
