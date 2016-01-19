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
		static bool includeInterfaces = false;
		string searchstring;

		static Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

		// maintain the target types and their parents, so we can hide the other types derived from the parents
		HashSet<Type> targetTypes = new HashSet<Type>();
		HashSet<Type> parentTypes = new HashSet<Type>();

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
			if ( parentTypes.Contains( entity ) )
				yield break;

			var subTypes = TypeUtility.GetSubtypes( entity, allAssemblies );
			foreach ( var t in subTypes )
				yield return GetRelation( entity, t );
		}

		// returns relations baseType->entity and interface->entity
		IEnumerable<Relation<Type, TypeRelation>> GetRelating( Type entity )
		{
			bool isParentOrTarget = parentTypes.Contains( entity ) || targetTypes.Contains( entity );

			if ( entity.BaseType != null && !entity.BaseType.IsInterface )
			{
				yield return GetRelation( entity.BaseType, entity );

				if ( isParentOrTarget )
					parentTypes.Add( entity.BaseType );
			}

			if ( includeInterfaces )
			{
				foreach ( var t in entity.GetInterfaces() )
				{
					yield return GetRelation( t, entity );

					if ( isParentOrTarget )
						parentTypes.Add( t );
				}
			}
		}

		static Relation<Type, TypeRelation> GetRelation( Type sourceType, Type targetType )
		{
			var relationType = sourceType.IsInterface ? 
				(targetType.IsInterface ? TypeRelation.Extension : TypeRelation.Implementation) : 
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

				GUILayout.FlexibleSpace();
				if ( !api.GetTargets().Any() && GUILayout.Button( "inspect Unity Object" ) )
					api.ResetTargets( new[] { typeof( UnityEngine.Object ) } );

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
				if( !api.GetTargets().Contains( single ) )
					menu.AddItem( new GUIContent( "inspect type " + single.Name ), false, () => api.ResetTargets( new[] { single } ) );
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
