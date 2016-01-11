using System.IO;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	internal class SkinManager
	{
		static RelationInspectorSkin LightSkin = Util.LoadOrCreate( ProjectSettings.LightSkinPath, () => CreateSkin( true ) );
		static RelationInspectorSkin DarkSkin = Util.LoadOrCreate( ProjectSettings.DarkSkinPath, () => CreateSkin( false ) );

		internal static RelationInspectorSkin GetSkin()
		{
			return EditorGUIUtility.isProSkin ? DarkSkin : LightSkin;
		}

		static RelationInspectorSkin CreateSkin( bool light )
		{
			var skin = ScriptableObject.CreateInstance<RelationInspectorSkin>();
			if ( light )
				PopulateLightSkinAsset( skin );
			else
				PopulateDarkSkinAsset( skin );
			return skin;
		}

		static void PopulateLightSkinAsset( RelationInspectorSkin skin )
		{
			skin.windowColor = new Color( 0.76f, 0.76f, 0.76f );
			skin.settingsIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "settingsIconLight.png" ) );
			skin.prevIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "prevIconLight.png" ) );
			skin.nextIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "nextIconLight.png" ) );

			skin.minimap = new MinimapStyle();
			skin.minimap.backgroundColor = new Color( 0.76f, 0.76f, 0.76f );
			skin.minimap.vertexMarkerColor = Color.black;
			skin.minimap.viewRectColor = Color.blue;
			skin.minimap.size = 75;
			skin.minimap.spacing = 15;

			skin.entityWidget = new EntityWidgetStyle();
			skin.entityWidget.contentStyle = new GUIStyle( GUI.skin.label );
			skin.entityWidget.contentStyle.normal.textColor = Color.black;
			skin.entityWidget.contentStyle.padding = new RectOffset( 6, 6, 6, 6 );
			skin.entityWidget.contentStyle.richText = true;
			skin.entityWidget.highlightStrength = 2;
			skin.entityWidget.widgetRadius = 9;
			skin.entityWidget.backgroundColor = new Color( 0.93f, 0.93f, 0.93f );
			skin.entityWidget.highlightColor = new Color( 0.35f, 0.55f, 1f );
			skin.entityWidget.shadowColor = new Color( 0, 0, 0, 0.5f );
			skin.entityWidget.unexploredColor = Color.red;
			skin.entityWidget.targetBackgroundColor = new Color( 0.55f, 0.55f, 0.55f );
			skin.entityWidget.discImage = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "disc.png" ) );

			skin.relationDrawer = new RelationDrawerStyle();
			skin.relationDrawer.markerSize = new Vector2( 15, 15 );
			skin.relationDrawer.regularEdgeColor = Color.black;
			skin.relationDrawer.regularEdgeWidth = 2;
			skin.relationDrawer.highlightEdgeColor = new Color( 0, 0.3f, 1, 1 );
			skin.relationDrawer.highlightEdgeWidth = 4;
			skin.relationDrawer.markerImage = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "ArrowHead.png" ) );
			skin.relationDrawer.edgeGapSize = 0;

			skin.tooltipStyle = new GUIStyle();
			skin.tooltipStyle.padding = new RectOffset( 3, 3, 3, 3 );
		}

		static void PopulateDarkSkinAsset( RelationInspectorSkin skin )
		{
			skin.windowColor = new Color( 0.2f, 0.2f, 0.2f );
			skin.settingsIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "settingsIconDark.png" ) );
			skin.prevIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "prevIconDark.png" ) );
			skin.nextIcon = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "nextIconDark.png" ) );

			skin.minimap = new MinimapStyle();
			skin.minimap.backgroundColor = new Color( 0.4f, 0.4f, 0.4f, 1f );
			skin.minimap.vertexMarkerColor = Color.white;
			skin.minimap.viewRectColor = new Color( 1f, 0.92f, 0f, 1f );
			skin.minimap.size = 75;
			skin.minimap.spacing = 15;

			skin.entityWidget = new EntityWidgetStyle();
			skin.entityWidget.contentStyle = new GUIStyle( GUI.skin.label );
			skin.entityWidget.contentStyle.normal.textColor = Color.white;
			skin.entityWidget.contentStyle.padding = new RectOffset( 6, 6, 6, 6 );
			skin.entityWidget.contentStyle.richText = true;
			skin.entityWidget.highlightStrength = 2;
			skin.entityWidget.widgetRadius = 9;
			skin.entityWidget.backgroundColor = new Color( 0.2f, 0.2f, 0.2f, 1f );
			skin.entityWidget.highlightColor = new Color( 0.78f, 0.72f, 0.08f, 1f );
			skin.entityWidget.shadowColor = new Color( 0, 0, 0, 0.5f );
			skin.entityWidget.unexploredColor = Color.red;
			skin.entityWidget.targetBackgroundColor = new Color( .45f, .45f, .45f );
			skin.entityWidget.discImage = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "disc.png" ) );

			skin.relationDrawer = new RelationDrawerStyle();
			skin.relationDrawer.markerSize = new Vector2( 15, 15 );
			skin.relationDrawer.regularEdgeColor = Color.white;
			skin.relationDrawer.regularEdgeWidth = 2;
			skin.relationDrawer.highlightEdgeColor = new Color( 1f, 0.92f, 0f, 1f );
			skin.relationDrawer.highlightEdgeWidth = 4;
			skin.relationDrawer.markerImage = Util.LoadAsset<Texture2D>( Path.Combine( ProjectSettings.ResourcesPath, "ArrowHead.png" ) );
			skin.relationDrawer.edgeGapSize = 0;

			skin.tooltipStyle = new GUIStyle();
			skin.tooltipStyle.normal.textColor = Color.white;
			skin.tooltipStyle.padding = new RectOffset( 3, 3, 3, 3 );
		}
	}
}
