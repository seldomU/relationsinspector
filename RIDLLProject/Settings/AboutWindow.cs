using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	class AboutWindow : EditorWindow
	{
		void OnGUI()
		{
#if RIDEMO
            string title = "Relations inspector demo";
#else
			string title = "Relations inspector";

#endif
			GUILayout.Label( title + " " + GetType().Assembly.GetName().Version.ToString() );
		}

		internal static void Spawn()
		{
			var window = GetWindow<AboutWindow>();
			window.title = "About";
			window.ShowUtility();
		}
	}
}
