using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
    class AboutWindow : EditorWindow
    {
        void OnGUI()
        {
            GUILayout.Label("Relations inspector v" + ProjectSettings.ProgramVersion);
        }

        internal static void Spawn()
        {
            var window = GetWindow<AboutWindow>();
            window.title = "About";
            window.ShowUtility();
        }
    }
}
