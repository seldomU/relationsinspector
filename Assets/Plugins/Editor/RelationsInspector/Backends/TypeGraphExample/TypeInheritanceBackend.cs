using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace RelationsInspector.Backend.TypeHierarchy
{
	// we have two kinds of relations: extending a base class and implementing an interface
	public enum TypeRelation { Extension, Implementation };

	[SaveLayout]
	public class TypeInheritanceBackend : MinimalBackend<Type, TypeRelation>
	{
		static MonoScript[] behaviourTypeAssets = MonoImporter.GetAllRuntimeMonoScripts();
		static bool includeInterfaces = false;
		string searchstring;

		static Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

		// maintain the target types and their parents, so we can hide the other types derived from the parents
		HashSet<Type> targetTypes = new HashSet<Type>();

		Dictionary<Type, int> touchedSubTypes = new Dictionary<Type, int>();
		Dictionary<Type, int> touchedSuperTypes = new Dictionary<Type, int>();
		static int subTypeDepth;
		const string subTypeDepthKey = "TypeInheritanceBackend" + "subTypeDepth";
		static int superTypeDepth;
		const string superTypeDepthKey = "TypeInheritanceBackend" + "superTypeDepth";

		public override void Awake( GetAPI getAPI )
		{
			subTypeDepth = EditorPrefs.GetInt( subTypeDepthKey, 2 );
			superTypeDepth = EditorPrefs.GetInt( superTypeDepthKey, 2 );
			base.Awake( getAPI );
		}

		public override IEnumerable<Type> Init( object target )
		{
			var type = target as Type;
			targetTypes.Add( type );
			return new[] { type };
		}

		public override IEnumerable<Relation<Type, TypeRelation>> GetRelations( Type entity )
		{
			return GetRelated( entity ).Concat( GetRelating( entity ) );
		}

		// returns relations entity->subtype, unless entity is the parent of a target
		IEnumerable<Relation<Type, TypeRelation>> GetRelated( Type entity )
		{
			// for superTypes of the targets, don't return subtypes
			if ( touchedSuperTypes.Keys.Contains( entity ) )
				yield break;

			if ( entity.IsInterface )
				yield break;

			int depth = touchedSubTypes.ContainsKey( entity ) ? touchedSubTypes[ entity ] + 1 : 1;
			if ( depth > subTypeDepth )
				yield break;

			var subTypes = TypeUtility.GetSubtypes( entity, allAssemblies );
			foreach ( var t in subTypes )
			{
				touchedSubTypes[ t ] = touchedSubTypes.ContainsKey( t ) ? Math.Min( depth, touchedSubTypes[ t ] ) : depth;
				yield return GetRelation( entity, t );
			}
		}

		// returns relations baseType->entity and interface->entity
		IEnumerable<Relation<Type, TypeRelation>> GetRelating( Type entity )
		{
			bool isParentOrTarget = touchedSuperTypes.ContainsKey( entity ) || targetTypes.Contains( entity );

			int depth = touchedSuperTypes.ContainsKey( entity ) ? touchedSuperTypes[ entity ] + 1 : 1;
			if ( depth > superTypeDepth )
				yield break;

			if ( entity.BaseType != null && !entity.BaseType.IsInterface )
			{
				yield return GetRelation( entity.BaseType, entity );

				if ( isParentOrTarget )
					touchedSuperTypes[ entity.BaseType ] = depth;
			}

			if ( includeInterfaces )
			{
				foreach ( var t in entity.GetInterfaces() )
				{
					yield return GetRelation( t, entity );

					if ( isParentOrTarget )
						touchedSuperTypes[ entity.BaseType ] = depth;
				}
			}
		}

		static Relation<Type, TypeRelation> GetRelation( Type sourceType, Type targetType )
		{
			var relationType = sourceType.IsInterface ?
				( targetType.IsInterface ? TypeRelation.Extension : TypeRelation.Implementation ) :
				TypeRelation.Extension;
			return new Relation<Type, TypeRelation>( sourceType, targetType, relationType );
		}

		public override Rect OnGUI()
		{
			GUILayout.BeginHorizontal( EditorStyles.toolbar );
			{
				// toggle interface type objects
				EditorGUI.BeginChangeCheck();
				{
					includeInterfaces = GUILayout.Toggle( includeInterfaces, "Include Interfaces", EditorStyles.toolbarButton );
				}
				if ( EditorGUI.EndChangeCheck() )
					api.Rebuild();

				EditorGUI.BeginChangeCheck();
				GUILayout.Space( 20 );
				GUILayout.Label( "Subtype depth", EditorStyles.miniLabel );
				//EditorGUIUtility.labelWidth = 110;
				subTypeDepth = EditorGUILayout.IntField( subTypeDepth, GUILayout.Width( 35 ) );
				GUILayout.Space( 20 );
				//EditorGUIUtility.labelWidth = 110;
				GUILayout.Label( "Supertype depth", EditorStyles.miniLabel );
				superTypeDepth = EditorGUILayout.IntField( superTypeDepth, GUILayout.Width(35) );
				if ( EditorGUI.EndChangeCheck() )
				{
					EditorPrefs.SetInt( subTypeDepthKey, subTypeDepth );
					EditorPrefs.SetInt( superTypeDepthKey, superTypeDepth );
					api.Rebuild();
					api.Relayout();
				}

				// search field
				GUILayout.FlexibleSpace();
				searchstring = BackendUtil.DrawEntitySelectSearchField( searchstring, api );
			}
			GUILayout.EndHorizontal();
			return BackendUtil.GetMaxRect();
		}


		public override void OnEntityContextClick( IEnumerable<Type> entities, GenericMenu menu )
		{
			if ( entities.Count() == 1 )
			{
				var single = entities.First();
				if ( !api.GetTargets().Contains( single ) )
					menu.AddItem( new GUIContent( "inspect type " + single.Name ), false, () => api.ResetTargets( new[] { single } ) );

				var typeAsset = behaviourTypeAssets.FirstOrDefault( monoScript => monoScript.GetClass() == single );
				if ( typeAsset != null )
					menu.AddItem( new GUIContent( "open " + single.Name ), false, () => AssetDatabase.OpenAsset( typeAsset ) );
				// UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal
			}
		}

		// map relation tag value to color
		public override Color GetRelationColor( TypeRelation relationTagValue )
		{
			switch ( relationTagValue )
			{
				case TypeRelation.Implementation:
					return Color.yellow;

				case TypeRelation.Extension:
				default:
					return Color.white;
			}
		}

		public override GUIContent GetContent( Type entity )
		{
			return new GUIContent
				(
				entity.Name,    // name
				AssetPreview.GetMiniTypeThumbnail( entity ), // icon
				entity.FullName // tooltil
				);
		}
	}
}
