using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelationsInspector
{
	public class RelationInspectorSkin : ScriptableObject
	{
		public MinimapStyle minimap;
		public EntityWidgetStyle entityWidget;
		public RelationDrawerStyle relationDrawer;
		public Color windowColor;
	}

	[Serializable]
	public class MinimapStyle
	{
		public Color backgroundColor;
		public Color vertexMarkerColor;
		public Color viewRectColor;
	}

	[Serializable]
	public class EntityWidgetStyle
	{
		public Color backgroundColor;
		public Color highlightColor;
		public float highlightStrength;
		public float contentPadding;
		public GUIStyle contentStyle;
		public float widgetRadius;	// for circle widget only
		public Color targetBackgroundColor;
	}

	[Serializable]
	public class RelationDrawerStyle
	{
		public Color regularEdgeColor;
		public int regularEdgeWidth;
		public Color highlightEdgeColor;
		public int highlightEdgeWidth;
		public Vector2 markerSize;
		public Texture2D markerImage;
	}
}
