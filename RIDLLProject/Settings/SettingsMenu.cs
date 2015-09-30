using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
    class SettingsMenu
    {
        const string DocURL = @"https://github.com/seldomU/RIBackendUtil/wiki";
        const string DiscussionURL = @"http://forum.unity3d.com/threads/demo-relations-inspector-asset.295319/";

        internal static void Create()
        {
            var menu = new GenericMenu();
            //menu.AddItem( new GUIContent( "Unity selection -> RI/On" ), false, () => { } );
            //menu.AddItem( new GUIContent( "Unity selection -> RI/Off" ), false, () => { } );
            //menu.AddItem( new GUIContent( "Unity selection -> RI/Backend decides" ), false, () => { } );
            menu.AddItem( new GUIContent( "Documentation" ), false, () => Application.OpenURL( DocURL ) );
            menu.AddItem( new GUIContent( "Discussion" ), false, () => Application.OpenURL( DiscussionURL ) );
            menu.AddItem( new GUIContent( "About" ), false, () => AboutWindow.Spawn() );
            menu.ShowAsContext();
        }
    }
}
