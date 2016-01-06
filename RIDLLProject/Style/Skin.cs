using UnityEngine;

namespace RelationsInspector
{
	public class RelationInspectorSkin : ScriptableObject
	{
		public MinimapStyle minimap;
		public EntityWidgetStyle entityWidget;
		public RelationDrawerStyle relationDrawer;
		public Color windowColor;
		public Texture2D settingsIcon;
		public Texture2D prevIcon;
		public Texture2D nextIcon;
		public GUIStyle tooltipStyle;
	}
}
