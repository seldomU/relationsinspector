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
			menu.AddItem( new GUIContent( "Info + Addons" ), false, () => WelcomeWindow.SpawnWindow() );
#if RIDEMO
			menu.AddItem( new GUIContent( "Buy RI" ), false, () => UnityEditorInternal.AssetStore.Open( ProjectSettings.StoreDemoURL ) );
#endif
			menu.ShowAsContext();
		}
	}
}
