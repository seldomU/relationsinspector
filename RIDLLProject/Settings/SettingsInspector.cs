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
        bool initialized;
        RelationsInspectorWindow riWindow;

        public override void OnInspectorGUI()
        {
            if ( !initialized )
            {
                initialized = true;
                riWindow = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>().FirstOrDefault();
            }

            GUILayout.Label( "Relations inspector settings", EditorStyles.boldLabel );

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if ( EditorGUI.EndChangeCheck() )
                RepaintRI();
        }

        void RepaintRI()
        {
            if ( riWindow != null )
                riWindow.Repaint();
        }
    }
}
