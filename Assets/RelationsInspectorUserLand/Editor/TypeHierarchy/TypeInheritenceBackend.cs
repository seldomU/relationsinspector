using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace RelationsInspector.Backend.TypeHierarchy
{
	// we have two kinds of relations in the inheritance graph:
	public enum TypeRelation { SubType, SuperType};

	public class TypeInheritenceBackend : MinimalBackend<Type, TypeRelation>
	{	
		static bool includeSuperTypes = true;
		static bool includeSubTypes = true;
		static bool includeInterfaces = true;

		static int maxNodes = 60;	// upper limit on number of graph nodes

		static Assembly[] gameAssemblies = new[]
		{
			typeof(GameObject).Assembly,
			typeof(UnityEditor.Editor).Assembly,
			TypeUtility.GetAssemblyByName("Assembly-CSharp-Editor"),
			TypeUtility.GetAssemblyByName("Assembly-CSharp")
		};

		static Assembly[] allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();

		static Dictionary<TypeRelation, Color> relationTypeColors = new Dictionary<TypeRelation, Color>()
		{
			{ TypeRelation.SubType, Color.magenta},
			{ TypeRelation.SuperType, Color.yellow}
		};

		int nodeCount = 0;	// number of graph nodes
		HashSet<Type> touchedSubTypes;
		HashSet<Type> touchedSuperTypes;
		object[] targets;

		public override IEnumerable<Type> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			var targetTypes = BackendUtil.Convert<Type>(targets);
			this.touchedSubTypes = new HashSet<Type>(targetTypes);
			this.touchedSuperTypes = new HashSet<Type>(targetTypes);
			this.targets = targets == null ? new Type[0] : targets.ToArray();
			return targetTypes;
		}

		public override IEnumerable<Tuple<Type, TypeRelation>> GetRelations(Type entity)
		{
			//
			foreach (var tuple in GetRelationTuples(entity))
			{
				if (nodeCount >= maxNodes)
					yield break;

				yield return tuple;
				nodeCount++;
			}
		}

		IEnumerable<Tuple<Type, TypeRelation>> GetRelationTuples(Type entity)
		{
			if (includeSubTypes && touchedSubTypes.Contains(entity))
			{
				var subTypes = TypeUtility.GetSubtypes(entity, allAssemblies);
				touchedSubTypes.UnionWith(subTypes);
				foreach (var t in subTypes)
					yield return new Tuple<Type, TypeRelation>(t, TypeRelation.SubType);
			}

			if (includeSuperTypes && touchedSuperTypes.Contains(entity))
			{
				var superTypes = new List<Type>();
				if (entity.BaseType != null)
					superTypes.Add(entity.BaseType);

				if (includeInterfaces)
					superTypes.AddRange(entity.GetInterfaces());

				touchedSuperTypes.UnionWith(superTypes);
				foreach (var t in superTypes)
					yield return new Tuple<Type, TypeRelation>(t, TypeRelation.SuperType);
			}
		}

		public override Rect OnGUI()
		{
			GUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				{
					GUI.contentColor = relationTypeColors[TypeRelation.SuperType];
					includeSuperTypes = GUILayout.Toggle(includeSuperTypes, "SuperTypes");

					GUI.contentColor = relationTypeColors[TypeRelation.SubType];
					includeSubTypes = GUILayout.Toggle(includeSubTypes, "SubTypes");

					GUI.contentColor = Color.white;
					includeInterfaces = GUILayout.Toggle(includeInterfaces, "Interfaces");
				}
				if (EditorGUI.EndChangeCheck())
					api.ResetTargets(targets);

				GUILayout.FlexibleSpace();
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

		// map relation tag value to color
		public override Color GetRelationColor(TypeRelation relationTagValue)
		{
			return relationTypeColors[ relationTagValue ];
		}
	}
}
