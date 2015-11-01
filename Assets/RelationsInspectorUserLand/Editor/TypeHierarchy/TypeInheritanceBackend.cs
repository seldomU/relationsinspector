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

	public class TypeInheritanceBackend : MinimalBackend<Type, TypeRelation>
	{	
		static bool includeSuperTypes = true;
		static bool includeSubTypes = true;
		static bool includeInterfaces = true;
		string searchstring;

		/*static Assembly[] gameAssemblies = new[]
		{
			typeof(GameObject).Assembly,
			typeof(UnityEditor.Editor).Assembly,
			TypeUtility.GetAssemblyByName("Assembly-CSharp-Editor"),
			TypeUtility.GetAssemblyByName("Assembly-CSharp")
		};*/

		static Assembly[] allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();

		static Dictionary<TypeRelation, Color> relationTypeColors = new Dictionary<TypeRelation, Color>()
		{
			{ TypeRelation.SubType, Color.magenta},
			{ TypeRelation.SuperType, Color.yellow}
		};

		HashSet<Type> touchedSubTypes;
		HashSet<Type> touchedSuperTypes;
		object[] targets;
        RelationsInspectorAPI riAPI;

		public override IEnumerable<Type> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
            this.riAPI = api;
            var targetTypes = (targets == null) ? Enumerable.Empty<Type>() : targets.OfType<Type>();
            this.touchedSubTypes = new HashSet<Type>(targetTypes);
			this.touchedSuperTypes = new HashSet<Type>(targetTypes);
			this.targets = targets == null ? new Type[0] : targets.ToArray();
			return targetTypes;
		}

        public override IEnumerable<Relation<Type, TypeRelation>> GetRelations( Type entity )
        {
            return GetRelated( entity ).Concat( GetRelating( entity ) );
        }

        IEnumerable<Relation<Type, TypeRelation>> GetRelated(Type entity)
		{
            if ( includeSubTypes && touchedSubTypes.Contains( entity ) )
            {
                var subTypes = TypeUtility.GetSubtypes( entity, allAssemblies );
                touchedSubTypes.UnionWith( subTypes );
                foreach ( var t in subTypes )
                    yield return new Relation<Type, TypeRelation>(entity, t, TypeRelation.SubType );
            }
        }

        IEnumerable<Relation<Type, TypeRelation>> GetRelating(Type entity)
        {
            if ( !includeSuperTypes || !touchedSuperTypes.Contains( entity ) )
                yield break;

            var superTypes = new List<Type>();
            if ( entity.BaseType != null )
                superTypes.Add( entity.BaseType );

            if ( includeInterfaces )
                superTypes.AddRange( entity.GetInterfaces() );

            touchedSuperTypes.UnionWith( superTypes );
            foreach ( var t in superTypes )
                yield return new Relation<Type, TypeRelation>( t, entity, TypeRelation.SuperType );
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
					riAPI.ResetTargets(targets);

				GUILayout.FlexibleSpace();
				EditorGUI.BeginChangeCheck();
				searchstring = EditorGUILayout.TextField(searchstring, GUI.skin.FindStyle("ToolbarSeachTextField"));	//EditorStyles.toolbarTextField);
				bool resetSearchString = GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"));
				if (EditorGUI.EndChangeCheck())
				{
					if (resetSearchString)
					{
						searchstring = string.Empty;
						GUI.FocusControl(null);
					}

					if (string.IsNullOrEmpty(searchstring))
						riAPI.SelectEntityNodes( x => { return false; } );
					else
						riAPI.SelectEntityNodes( x => (x as Type).FullName.Contains(searchstring) );
				}

			}
			GUILayout.EndHorizontal();
			return BackendUtil.GetMaxRect();
		}



		public override void OnEntityContextClick(IEnumerable<Type> entities, GenericMenu menu)
		{
			menu.AddItem(new GUIContent("make new target"), false, () => riAPI.ResetTargets(entities.ToArray() ));
		}

		// map relation tag value to color
		public override Color GetRelationColor(TypeRelation relationTagValue)
		{
			return relationTypeColors[ relationTagValue ];
		}

		public override GUIContent GetContent(Type entity)
		{
			return new GUIContent
				(
				entity.Name,	// name
				AssetPreview.GetMiniTypeThumbnail(entity), // icon
				entity.FullName	// tooltil
				);
		}
	}
}
