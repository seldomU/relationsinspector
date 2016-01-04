using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace RelationsInspector.Backend.Scene
{
    public class TagBackend : MinimalBackend<Object,string>
    {
        List<Object> tagObjects = new List<Object>();
        string searchstring;

        public override IEnumerable<Object> Init( object target )
        {
            var asGo = target as GameObject;
            if ( asGo == null )
                yield break;

            yield return asGo;
            yield return GetTagObj( asGo.tag );
        }

        public override IEnumerable<Relation<Object, string>> GetRelations( Object entity )
        {
            var asGO = entity as GameObject;
            if ( asGO != null )
                yield return new Relation<Object, string>( GetTagObj( asGO.tag ), asGO, string.Empty );
        }

        Object GetTagObj( string tag )
        {
            string tagTitle = "Tag: " + tag;

            var obj = tagObjects.FirstOrDefault( o => o.name == tagTitle );
            if ( obj == null )
            {
                obj = Object.Instantiate( EditorGUIUtility.whiteTexture );
                obj.name = tagTitle;
                tagObjects.Add( obj );
            }
            return obj;
        }

        bool ContainsUntaggedTargets()
        {
            // if there are, there must be a tag object for them
            return tagObjects != null && tagObjects.Any( o => o.name == "Tag: Untagged" );
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
                if( ContainsUntaggedTargets() &&
                    GUILayout.Button( "Hide untagged", EditorStyles.toolbarButton ) )
                    api.ResetTargets( api.GetTargets().Where( IsUntagged ).ToArray() );

                GUILayout.FlexibleSpace();

                searchstring = BackendUtil.DrawEntitySelectSearchField( searchstring, api );
            }
            GUILayout.EndHorizontal();
            return base.OnGUI();
        }

        public override GUIContent GetContent( Object entity )
        {
            return new GUIContent( entity.name, null, entity.name );
        }

        public override void OnEntitySelectionChange( Object[] selection )
        {
            base.OnEntitySelectionChange( selection.Except( tagObjects ).ToArray() );
        }
    }
}
