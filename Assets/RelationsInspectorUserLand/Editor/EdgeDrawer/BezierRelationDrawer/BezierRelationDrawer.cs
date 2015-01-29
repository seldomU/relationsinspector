/*
using UnityEngine;
using System.Collections;
using UnityEditor;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public class BezierRelationDrawer<T> : BasicRelationDrawer<T>
	{
		protected override Rect DrawRegularRelation(T value, EdgePlacement placement, bool highlight, bool includeMarker, RelationDrawerStyle style)
		{
			//var center = (placement.startPos + placement.endPos) * 0.5f;

			var startTangent = placement.startPos + placement.startNormal * 30;
			var endTangent = placement.endPos + placement.endNormal * 30;
			var linecolor = highlight ? style.highlightEdgeColor : style.regularEdgeColor;
			Handles.DrawBezier(placement.startPos, placement.endPos, startTangent, endTangent, linecolor, null, 3);
		
			//var markerRect = Util.CenterRect(center, markerSize);
			if (includeMarker)
				return DrawMarker(placement.startPos, placement.endPos, style.markerSize, style.markerImage );

			return Util.rectZero;
		}

		protected override Rect DrawSelfRelation(T value, EdgePlacement placement, bool highlight, bool includeMarker, RelationDrawerStyle style)
		{
			var color = highlight ? style.highlightEdgeColor : style.regularEdgeColor;
			// leave half a marker's size of margin between top-left corner and marker
			var markerCenter = new Vector2(placement.startPos.x, placement.endPos.y) - style.markerSize;
			var markerRect = Util.CenterRect(markerCenter, style.markerSize);
			if (includeMarker)
				EditorGUI.DrawRect(markerRect, color);

			Vector2 startTangent = placement.startPos + placement.startNormal * style.markerSize.x;
			Vector2 endTangent = placement.endPos + placement.endNormal * style.markerSize.y;

			Vector2 markerAnchorBottom = new Vector2(markerRect.center.x, markerRect.yMax);
			Vector2 markerTangentBottom = markerAnchorBottom + Vector2.up * style.markerSize.y;

			Vector2 markerAnchorRight = new Vector2(markerRect.xMax, markerRect.center.y);
			Vector2 markerTangentRight = markerAnchorRight + Vector2.right * style.markerSize.x;

			Handles.DrawBezier(placement.startPos, markerAnchorBottom, startTangent, markerTangentBottom, color, null, 3);
			Handles.DrawBezier(placement.endPos, markerAnchorRight, endTangent, markerTangentRight, color, null, 3);
			return markerRect;
		}
	}
}
*/
