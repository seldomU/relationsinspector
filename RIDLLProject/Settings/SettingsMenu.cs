using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	class SettingsMenu
	{
		internal static void Create()
		{
			var menu = new GenericMenu();
			menu.AddItem( new GUIContent( "Settings" ), false, () => Selection.activeObject = Settings.Instance );
			menu.AddItem( new GUIContent( "Skin" ), false, () => Selection.activeObject = SkinManager.GetSkin() );
			menu.AddItem( new GUIContent( "About" ), false, () => WelcomeWindow.Spawn() );
#if RIDEMO
			menu.AddItem( new GUIContent( "Buy RI" ), false, () => UnityEditorInternal.AssetStore.Open( ProjectSettings.StoreURL ) );
#endif
			menu.ShowAsContext();
		}
	}
}
