using System;
using UnityEngine;

namespace RelationsInspector
{
	[Serializable]
	public class RelationDrawerStyle
	{
		public Color regularEdgeColor;
		public int regularEdgeWidth;
		public Color highlightEdgeColor;
		public int highlightEdgeWidth;
		public Vector2 markerSize;
		public Texture2D markerImage;
		public float edgeGapSize;
	}
}
