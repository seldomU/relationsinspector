using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
    [CustomEditor(typeof(RelationsInspectorSettings))]
    class SettingsInspector : Editor
    {
        RelationsInspectorWindow riWindow;
        RelationsInspectorSettings settings;
        bool foldGraphSettings;

        void OnEnable()
        {
            riWindow = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>().FirstOrDefault();
            settings = target as RelationsInspectorSettings;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label( "Relations inspector settings", EditorStyles.boldLabel );
            EditorGUILayout.Space();

            settings.maxGraphNodes = EditorGUILayout.IntField( "Max graph nodes", settings.maxGraphNodes );

            EditorGUI.BeginChangeCheck();
            settings.treeRootLocation = (TreeRootLocation) EditorGUILayout.EnumPopup( "Tree root location", settings.treeRootLocation );
            if ( EditorGUI.EndChangeCheck() && riWindow != null )
                riWindow.GetAPI().Relayout();

            EditorGUI.BeginChangeCheck();
            settings.showMinimap = EditorGUILayout.Toggle( "Show minimap", settings.showMinimap );
            settings.minimapLocation = (MinimapLocation) EditorGUILayout.EnumPopup( "Minimap location", settings.minimapLocation );
            if ( EditorGUI.EndChangeCheck() && riWindow != null )
                riWindow.GetAPI().Repaint();

            settings.logToConsole = EditorGUILayout.Toggle( "Log to console", settings.logToConsole );

#if DEBUG
            ShowLayoutParams( settings.layoutParams );
#endif           
            if ( GUI.changed )
                EditorUtility.SetDirty( settings );             
        }

        void ShowLayoutParams(GraphLayoutParams lParams)
        {
            EditorGUILayout.Space();
            foldGraphSettings = EditorGUILayout.Foldout( foldGraphSettings, "Graph layout settings" );
            if ( !foldGraphSettings )
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space( 10 );
                GUILayout.BeginVertical();
             
                lParams.maxFrameDuration = EditorGUILayout.FloatField( "Frame duration", lParams.maxFrameDuration );

                EditorGUILayout.Space();
                GUILayout.Label( "Position tweens", EditorStyles.boldLabel );
                lParams.vertexPosTweenDuration = EditorGUILayout.FloatField( "Duration", lParams.vertexPosTweenDuration );
                lParams.vertexPosTweenUpdateInterval = EditorGUILayout.FloatField( "Update interval", lParams.vertexPosTweenUpdateInterval );

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }
    }
}
