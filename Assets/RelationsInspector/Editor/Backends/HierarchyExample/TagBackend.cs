using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace RelationsInspector.Backend.Scene
{
    public class TagBackend : MinimalBackend<Object,string>
    {
        List<Object> tagObjects;
        string searchstring;

        public override IEnumerable<Relation<Object, string>> GetRelations( Object entity )
        {
            var asGO = entity as GameObject;
            if ( asGO != null )
                yield return new Relation<Object, string>( asGO, GetTagObj( asGO.tag ), string.Empty );
        }

        Object GetTagObj( string tag )
        {
            if ( tagObjects == null )
                tagObjects = new List<Object>();

            var obj = tagObjects.FirstOrDefault( o => o.name == tag );
            if ( obj == null )
            {
                obj = Object.Instantiate( EditorGUIUtility.whiteTexture );  // any dummy object will do.
                obj.name = "Tag: " + tag;
                tagObjects.Add( obj );
            }
            return obj;
        }

        public override Rect OnGUI()
        {
            GUILayout.BeginHorizontal( EditorStyles.toolbar );
            {
                if ( GUILayout.Button( "Show active scene", EditorStyles.toolbarButton ) )
                {
                    var sceneGOs = Object.FindObjectsOfType<GameObject>();
                    api.ResetTargets( sceneGOs.Cast<object>().ToArray() );
                }

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
