using System.IO;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	public class SkinManager
	{

		const string darkSkinName = "RIWindowDarkSkin";
		const string lightSkinName = "RIWindowLightSkin";

        // public so build tool code can use these
		public static string LightSkinPath = Path.Combine(ProjectSettings.ResourcesPath, lightSkinName + ".asset");
		public static string DarkSkinPath = Path.Combine(ProjectSettings.ResourcesPath, darkSkinName + ".asset");

		static RelationInspectorSkin LightSkin = LoadSkin(lightSkin: true);
		static RelationInspectorSkin DarkSkin = LoadSkin(lightSkin: false);

		internal static RelationInspectorSkin GetSkin()
		{
			return EditorGUIUtility.isProSkin ? DarkSkin : LightSkin;
		}

		static RelationInspectorSkin LoadSkin(bool lightSkin)
		{
			string path = lightSkin ? LightSkinPath : DarkSkinPath;
			path = path.Replace('\\', '/');
			if (File.Exists(path))
				return Util.LoadAsset<RelationInspectorSkin>(path);

			var skin = ScriptableObject.CreateInstance<RelationInspectorSkin>();
			if (lightSkin)
				PopulateLightSkinAsset(skin);
			else
				PopulateDarkSkinAsset(skin);		

			AssetDatabase.CreateAsset(skin, path);
			AssetDatabase.SaveAssets();
			return skin;
		}

		static void PopulateLightSkinAsset(RelationInspectorSkin skin)
		{
			skin.windowColor = new Color(0.76f, 0.76f, 0.76f);
            skin.settingsIcon = EditorGUIUtility.Load( "icons/_Popup.png" ) as Texture2D;
            skin.prevIcon = EditorGUIUtility.Load( "icons/Profiler.PrevFrame.png" ) as Texture2D;
            skin.nextIcon = EditorGUIUtility.Load( "icons/Profiler.NextFrame.png" ) as Texture2D;

            skin.minimap = new MinimapStyle();
			skin.minimap.backgroundColor = new Color(0.76f, 0.76f, 0.76f);
			skin.minimap.vertexMarkerColor = Color.black;
			skin.minimap.viewRectColor = Color.blue;
            skin.minimap.size = 75;
            skin.minimap.spacing = 15;

			skin.entityWidget = new EntityWidgetStyle();
			skin.entityWidget.contentStyle = new GUIStyle(GUI.skin.label);
			skin.entityWidget.contentStyle.normal.textColor = Color.black;
			skin.entityWidget.contentPadding = 4;
			skin.entityWidget.highlightStrength = 4;
			skin.entityWidget.widgetRadius = 13;
			skin.entityWidget.backgroundColor = new Color(0.76f, 0.76f, 0.76f);
			skin.entityWidget.highlightColor = new Color(0.35f, 0.55f, 1f);
            skin.entityWidget.unexploredColor = Color.red;
			skin.entityWidget.targetBackgroundColor = Color.white;

			skin.relationDrawer = new RelationDrawerStyle();
			skin.relationDrawer.markerSize = new Vector2(15, 15);
			skin.relationDrawer.regularEdgeColor = Color.black;
			skin.relationDrawer.regularEdgeWidth = 2;
			skin.relationDrawer.highlightEdgeColor = new Color(0, 0.3f, 1, 1);
			skin.relationDrawer.highlightEdgeWidth = 4;
			skin.relationDrawer.markerImage = Util.LoadAsset<Texture2D>( Path.Combine(ProjectSettings.ResourcesPath, "ArrowHead.png"));
		}

		static void PopulateDarkSkinAsset(RelationInspectorSkin skin)
		{
			skin.windowColor = new Color(0.2f, 0.2f, 0.2f);
            skin.settingsIcon = EditorGUIUtility.Load( "icons/d__Popup.png" ) as Texture2D;
            skin.prevIcon = EditorGUIUtility.Load( "icons/d_Profiler.PrevFrame.png" ) as Texture2D;
            skin.nextIcon = EditorGUIUtility.Load( "icons/d_Profiler.NextFrame.png" ) as Texture2D;

            skin.minimap = new MinimapStyle();
			skin.minimap.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1f);
			skin.minimap.vertexMarkerColor = Color.white;
			skin.minimap.viewRectColor = new Color(1f, 0.92f, 0f, 1f);
            skin.minimap.size = 75;
            skin.minimap.spacing = 15;

            skin.entityWidget = new EntityWidgetStyle();
			skin.entityWidget.contentStyle = new GUIStyle(GUI.skin.label);
			skin.entityWidget.contentStyle.normal.textColor = Color.white;
			skin.entityWidget.contentPadding = 4;
			skin.entityWidget.highlightStrength = 4;
			skin.entityWidget.widgetRadius = 13;
			skin.entityWidget.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			skin.entityWidget.highlightColor = new Color(0.78f, 0.72f, 0.08f, 1f);
            skin.entityWidget.unexploredColor = Color.red;
            skin.entityWidget.targetBackgroundColor = new Color(.45f, .45f, .45f);

			skin.relationDrawer = new RelationDrawerStyle();
			skin.relationDrawer.markerSize = new Vector2(15, 15);
			skin.relationDrawer.regularEdgeColor = Color.white;
			skin.relationDrawer.regularEdgeWidth = 2;
			skin.relationDrawer.highlightEdgeColor = new Color(1f, 0.92f, 0f, 1f);
			skin.relationDrawer.highlightEdgeWidth = 4;
			skin.relationDrawer.markerImage = Util.LoadAsset<Texture2D>(Path.Combine(ProjectSettings.ResourcesPath, "ArrowHead.png"));
		}
	}
}
