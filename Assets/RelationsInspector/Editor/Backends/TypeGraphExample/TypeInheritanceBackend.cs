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

		HashSet<Type> parentTypes = new HashSet<Type>();

		public override IEnumerable<Relation<Type, TypeRelation>> GetRelations( Type entity )
		{
			return GetRelated( entity ).Concat( GetRelating( entity ) );
		}

		IEnumerable<Relation<Type, TypeRelation>> GetRelated( Type entity )
		{
			if ( parentTypes.Contains( entity ) )
				yield break;

			var subTypes = TypeUtility.GetSubtypes( entity, allAssemblies );
			foreach ( var t in subTypes )
				yield return new Relation<Type, TypeRelation>( entity, t, TypeRelation.Extension );
		}

		IEnumerable<Relation<Type, TypeRelation>> GetRelating( Type entity )
		{
			if ( entity.BaseType != null )
			{
				parentTypes.Add( entity.BaseType );
				yield return new Relation<Type, TypeRelation>( entity.BaseType, entity, TypeRelation.Extension );
			}

			if ( includeInterfaces )
			{
				foreach ( var t in entity.GetInterfaces() )
				{
					parentTypes.Add( t );
					yield return new Relation<Type, TypeRelation>( t, entity, TypeRelation.Implementation );

				}
			}
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
			var single = entities.SingleOrDefault();
			if ( single != null && !api.GetTargets().Contains( single ) )
				menu.AddItem( new GUIContent( "inspect type " + single.Name ), false, () => api.ResetTargets( new[] { single } ) );
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
