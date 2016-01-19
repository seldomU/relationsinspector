using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	[CustomEditor( typeof( RelationsInspectorSettings ) )]
	class SettingsInspector : Editor
	{
		RelationsInspectorAPI  api;
		RelationsInspectorSettings settings;
		bool foldLayoutTweenSettings;
		bool foldLayoutSettings;

		void OnEnable()
		{
			var riWindow = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>().FirstOrDefault();
			api = riWindow == null ? null : riWindow.GetAPI( 1 ) as RelationsInspectorAPI;
			settings = target as RelationsInspectorSettings;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Label( "Relations inspector settings", EditorStyles.boldLabel );
			EditorGUILayout.Space();

			settings.cacheLayouts = EditorGUILayout.Toggle( "Cache layouts", settings.cacheLayouts );
			settings.maxGraphNodes = EditorGUILayout.IntField( "Max graph nodes", settings.maxGraphNodes );

			EditorGUI.BeginChangeCheck();
			settings.treeRootLocation = (TreeRootLocation) EditorGUILayout.EnumPopup( "Tree root location", settings.treeRootLocation );
			if ( EditorGUI.EndChangeCheck() && api != null )
				api.Relayout();

			EditorGUI.BeginChangeCheck();
			settings.showMinimap = EditorGUILayout.Toggle( "Show minimap", settings.showMinimap );
			settings.minimapLocation = (MinimapLocation) EditorGUILayout.EnumPopup( "Minimap location", settings.minimapLocation );
			if ( EditorGUI.EndChangeCheck() && api != null )
				api.Repaint();

			settings.logToConsole = EditorGUILayout.Toggle( "Log to console", settings.logToConsole );
			settings.invertZoom = EditorGUILayout.Toggle( "Invert zoom", settings.invertZoom );
#if DEBUG
			settings.repaintEachFrame = EditorGUILayout.Toggle( "Repaint each frame", settings.repaintEachFrame );
			ShowGraphLayoutParams( settings.graphLayoutParameters );
			ShowLayoutTweenParams( settings.layoutTweenParameters );
#endif
			if ( GUI.changed )
				EditorUtility.SetDirty( settings );
		}

		void ShowGraphLayoutParams( GraphLayoutParameters parameters )
		{
			EditorGUILayout.Space();
			foldLayoutSettings = EditorGUILayout.Foldout( foldLayoutSettings, "Graph layout settings" );
			if ( foldLayoutSettings )
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space( 10 );
				GUILayout.BeginVertical();

				parameters.posInitRange = EditorGUILayout.FloatField( "Init positioning range", parameters.posInitRange );
				parameters.idealDistance = EditorGUILayout.FloatField( "Ideal vertex spacing", parameters.idealDistance );
				parameters.initalMaxMove = EditorGUILayout.FloatField( "Inital max move", parameters.initalMaxMove );
				parameters.numIterations = EditorGUILayout.IntField( "Number of iterations", parameters.numIterations );
				parameters.gravityStrength = EditorGUILayout.FloatField( "Gravity strength", parameters.gravityStrength );

				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		void ShowLayoutTweenParams( LayoutTweenParameters lParams )
		{
			EditorGUILayout.Space();
			foldLayoutTweenSettings = EditorGUILayout.Foldout( foldLayoutTweenSettings, "Layout tween settings" );
			if ( foldLayoutTweenSettings )
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
