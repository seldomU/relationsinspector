using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RelationsInspector.Backend.Scene
{
	// don't save layouts. graphs will have identical seeds (scene obj), but different content
	[SaveLayout( false )]
	public class SceneHierarchyBackend : IGraphBackend<Object, string>
	{
		RelationsInspectorAPI api;
		Object sceneObj;    // representing the scene, as a parent for all the top-level GameObjects in the hierarchy
		static bool includeComponents;

		public void Awake( GetAPI getAPI )
		{
			api = getAPI(1) as RelationsInspectorAPI;
			sceneObj = new GameObject();
			sceneObj.name = "Scene";
			sceneObj.hideFlags = HideFlags.HideAndDontSave;
		}

		bool IsSceneObject( Object obj )
		{
			return obj != null && obj.name == "Scene" && obj.hideFlags == HideFlags.HideAndDontSave;
		}

		public IEnumerable<Object> Init( object target )
		{
			return ( target is Object ) ? new Object[] { target as Object } : new Object[ 0 ];
		}

		// this backend is only for display
		// so don't perform any graph modification
		public void OnDestroy() { }
		public void CreateEntity( Vector2 position ) { }
		public void CreateRelation( Object source, Object target ) { }
		public void OnRelationContextClick( Relation<Object, string> relation, GenericMenu contextMenu ) { }

		public void OnEntityContextClick( IEnumerable<Object> entities, GenericMenu contextMenu )
		{
			if(entities.Count() == 1)
			{
				var single = entities.First();
				var type = single.GetType();
				contextMenu.AddItem( new GUIContent( "inspect type " + type.Name ), false, () => api.ResetTargets( new[] { type } ) );
			}
		}


		// no need for toolbar or controls
		public Rect OnGUI()
		{
			GUILayout.BeginHorizontal( EditorStyles.toolbar );
			{
				if ( GUILayout.Button( "Show active scene", EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
				{
					api.ResetTargets( new object[] { sceneObj } );
				}

				EditorGUI.BeginChangeCheck();
				includeComponents = GUILayout.Toggle( includeComponents, "Include components" );
				if ( EditorGUI.EndChangeCheck() )
				{
					api.Rebuild();
				}

				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
			return BackendUtil.GetMaxRect();
		}

		// draw content
		public Rect DrawContent( Object entity, EntityDrawContext drawContext )
		{
			return DrawUtil.DrawContent( GetContent( entity ), drawContext );
		}

		// 
		public Color GetRelationColor( string relationTagValue )
		{
			return Color.white;
		}

		public string GetEntityTooltip( Object entity )
		{
			return GetContent( entity ).tooltip;
		}

		public string GetTagTooltip( string tag )
		{
			return tag;
		}

		public void OnEntitySelectionChange( Object[] selection )
		{
			// forward our selection to unity's selection
			Selection.objects = selection.Where(o => !IsSceneObject( o ) ).ToArray();
		}

		public virtual void OnUnitySelectionChange() { }

		public IEnumerable<Relation<Object, string>> GetRelations( Object entity )
		{
			// the fake scene object gets special care
			if ( IsSceneObject( entity ) )
			{
				var allGOs = Object.FindObjectsOfType<GameObject>();
				var rootGOs = allGOs.Where( go => go.transform.parent == null );
				foreach ( var go in rootGOs )
					yield return new Relation<Object, string>( entity, go, string.Empty );

				yield break;
			}

			var asGameObject = entity as GameObject;
			if ( asGameObject == null )
				yield break;

			// include sub-gameObjects
			foreach ( Transform t in asGameObject.transform )
				yield return new Relation<Object, string>( entity, t.gameObject, string.Empty );

			// include components
			if ( includeComponents )
			{
				foreach ( var c in asGameObject.GetComponents<Component>() )
					yield return new Relation<Object, string>( entity, c, string.Empty );
			}
		}

		public GUIContent GetContent( Object entity )
		{
			if( IsSceneObject( entity ) )
				return new GUIContent( "Scene", null, "The active scene" );

			var content = EditorGUIUtility.ObjectContent( entity, entity.GetType() );

			// components get their parent object's name as text
			// replace it with their type name
			if ( entity is Component )
				content.text = entity.GetType().Name;

			content.tooltip = content.text;
			return content;
		}

		// event handler for generic commands
		public virtual void OnCommand( string command ) { }
	}
}
