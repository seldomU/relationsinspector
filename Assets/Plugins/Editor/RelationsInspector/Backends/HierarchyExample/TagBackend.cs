using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace RelationsInspector.Backend.Scene
{
	public class Tag
	{
		public string value;
	}

	public class TagBackend : MinimalBackend<object, string>
	{
		List<Tag> tagObjects = new List<Tag>();
		string searchstring;

		public override IEnumerable<object> Init( object target )
		{
			var asGo = target as GameObject;
			if ( asGo == null )
				yield break;

			yield return asGo;
			yield return GetTagObj( asGo.tag );
		}

		public override IEnumerable<Relation<object, string>> GetRelations( object entity )
		{
			var asGO = entity as GameObject;
			if ( asGO != null )
				yield return new Relation<object, string>( GetTagObj( asGO.tag ), asGO, string.Empty );
		}

		Tag GetTagObj( string tag )
		{
			string tagTitle = "Tag: " + tag;

			var obj = tagObjects.FirstOrDefault( o => o.value == tagTitle );
			if ( obj == null )
			{
				obj = new Tag() { value = tagTitle };//Object.Instantiate( EditorGUIUtility.whiteTexture );
				tagObjects.Add( obj );
			}
			return obj;
		}

		bool ContainsUntaggedTargets()
		{
			// if there are, there must be a tag object for them
			return tagObjects != null && tagObjects.Any( o => o.value == "Tag: Untagged" );
		}

		bool IsUntagged( object obj )
		{
			var asGameObject = obj as GameObject;
			return asGameObject != null && asGameObject.tag != "Untagged";
		}

		public override Rect OnGUI()
		{
			GUILayout.BeginHorizontal( EditorStyles.toolbar );
			{
				// option: use all gameobjects of the active scene as targets
				if ( GUILayout.Button( "Show active scene", EditorStyles.toolbarButton ) )
					api.ResetTargets( Object.FindObjectsOfType<GameObject>().Cast<object>().ToArray() );

				// option: remove untagged objects
				if ( ContainsUntaggedTargets() &&
					GUILayout.Button( "Hide untagged", EditorStyles.toolbarButton ) )
					api.ResetTargets( api.GetTargets().Where( IsUntagged ).ToArray() );

				GUILayout.FlexibleSpace();

				searchstring = BackendUtil.DrawEntitySelectSearchField( searchstring, api );
			}
			GUILayout.EndHorizontal();
			return base.OnGUI();
		}

		public override GUIContent GetContent( object entity )
		{
			var asGo = entity as GameObject;
			if ( asGo != null )
				return new GUIContent( asGo.name, null, asGo.name );

			var asTag = entity as Tag;
			if ( asTag != null )
				return new GUIContent( asTag.value, null, asTag.value );

			return new GUIContent( "unknown object " + entity.ToString(), null, string.Empty );
		}

		public override void OnEntitySelectionChange( object[] selection )
		{
			base.OnEntitySelectionChange( selection.OfType<GameObject>().ToArray() );
		}
	}
}
