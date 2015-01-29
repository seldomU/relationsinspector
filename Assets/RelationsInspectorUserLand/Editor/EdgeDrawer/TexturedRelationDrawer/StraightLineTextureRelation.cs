/*
using UnityEngine;
using System.Collections;
using UnityEditor;
using RelationsInspector;

public class StraightLineTextureEdge<T> : BasicTagDrawer<T>
{
	const float edgeWidth = 10;
	Vector2 selfEdgeSize = new Vector2(35, 35);
	static string regularTexturePath =	Application.dataPath + @"RelationsInspector\Editor\TexturedRelationDrawer\RegularEdge.png";
	static string selfTexturePath =		Application.dataPath + @"RelationsInspector\Editor\TexturedRelationDrawer\SelfEdge.png";

	static Texture2D _regularEdgeTexture;
	static Texture2D RegularEdgeTexture
	{
		get
		{
			if( _regularEdgeTexture == null)
				_regularEdgeTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(regularTexturePath, typeof(Texture2D));
			return _regularEdgeTexture;
		}
	}

	static Texture2D _selfEdgeTexture;
	static Texture2D SelfEdgeTexture
	{
		get
		{
			if (_selfEdgeTexture == null)
				_selfEdgeTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(selfTexturePath, typeof(Texture2D));
			return _selfEdgeTexture;
		}
	}

	protected override Rect DrawRegularEdge(T value, EdgePlacement placement, bool includeMarker)
	{
		var center = (placement.endPos + placement.startPos) * 0.5f;
		var distance = (placement.endPos - placement.startPos).magnitude;
		float rotation = Util.GetAngle(placement.endPos - placement.startPos);
		EdgeDrawUtil.DrawRotatedTexture(RegularEdgeTexture, center, new Vector2(distance, edgeWidth), rotation);

		var markerRect = Util.CenterRect(center, markerSize);
		if (includeMarker)
		{
			EdgeDrawUtil.DrawRotatedRect(markerRect, rotation, Color.white);
		}

		return markerRect;
	}

	protected override Rect DrawSelfEdge(T value, EdgePlacement placement, bool includeMarker)
	{
		var center = new Vector2(placement.startPos.x, placement.endPos.y);

		var rect = Util.CenterRect(center, selfEdgeSize);
		GUI.DrawTexture(rect, SelfEdgeTexture, ScaleMode.ScaleToFit, true);

		// leave half a marker's size of margin between top-left corner and marker
		var markerCenter = center - markerSize;
		var markerRect = Util.CenterRect(markerCenter, markerSize);
		if (includeMarker)
			EditorGUI.DrawRect(markerRect, Color.white);

		return markerRect;
	}
}
*/