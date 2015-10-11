using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace RelationsInspector
{
    [CustomEditor(typeof(RelationsInspectorSettings))]
    class SettingsInspector : Editor
    {
        RelationsInspectorWindow riWindow;
        RelationsInspectorSettings settings;

        void OnEnable()
        {
            riWindow = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>().FirstOrDefault();
            settings = target as RelationsInspectorSettings;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label( "Relations inspector settings", EditorStyles.boldLabel );
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            settings.treeRootLocation = (TreeRootLocation) EditorGUILayout.EnumPopup( "Tree root location", settings.treeRootLocation );
            if ( EditorGUI.EndChangeCheck() && riWindow != null )
                riWindow.GetAPI().Relayout();

            EditorGUI.BeginChangeCheck();
            settings.showMinimap = EditorGUILayout.Toggle( "Show minimap", settings.showMinimap );
            settings.minimapLocation = (MinimapLocation) EditorGUILayout.EnumPopup( "Minimap location", settings.minimapLocation );
            if ( EditorGUI.EndChangeCheck() && riWindow != null )
                riWindow.GetAPI().Repaint();
        }

    }
}
