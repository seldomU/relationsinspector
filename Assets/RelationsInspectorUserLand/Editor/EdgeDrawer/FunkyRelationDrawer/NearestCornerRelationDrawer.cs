/*
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RelationsInspector
{
	public class NearestCornerEdgeDrawer<T> : BasicTagDrawer<T>
	{
		static float edgeWidth = 8;
		
		protected override Rect DrawRegularEdge(T value, EdgePlacement placement, bool includeMarker)
		{
			DrawArcEdge(placement.startPos, placement.endPos);

			Vector2 center = (placement.startPos + placement.endPos) * 0.5f;
			var markerRect = Util.CenterRect(center, markerSize);
			if( includeMarker)
				EditorGUI.DrawRect(markerRect, Color.white);
			return markerRect;
		}

		protected override Rect DrawSelfEdge(T value, EdgePlacement placement, bool includeMarker)
		{
			// leave half a marker's size of margin between top-left corner and marker
			var markerCenter = new Vector2(placement.startPos.x, placement.endPos.y) - markerSize;
			var markerRect = Util.CenterRect(markerCenter, markerSize);
			if (includeMarker)
				EditorGUI.DrawRect(markerRect, Color.white);
			Handles.DrawLine(placement.startPos, new Vector2(markerRect.center.x, markerRect.yMax));
			Handles.DrawLine(placement.endPos, new Vector2(markerRect.xMax, markerRect.center.y));
			return markerRect;
		}

		public static void DrawLineEdge(Vector2 from, Vector2 to)
		{
			Handles.DrawLine(from, to);
		}

		public static void DrawArcEdge(Vector2 from, Vector2 to)
		{
			// draw an arc from source to target, and a circle at the end
			Vector2 direction = (to - from).normalized;

			Vector2 orthoCounterClockWise = new Vector2(-direction.y, direction.x);

			Vector2 circleCenter = to - (direction * edgeWidth / 2);
			Vector2 arcStartPoint = circleCenter + (orthoCounterClockWise * edgeWidth / 2);
			Vector2 arcstartDirection = arcStartPoint - from;
			float arcRadius = arcstartDirection.magnitude;

			float arcStartAngle = Mathf.Atan2(arcstartDirection.y, arcstartDirection.x);
			float directionAngle = Mathf.Atan2(direction.y, direction.x);
			float arcAngle = Util.NormalizeAngle((arcStartAngle - directionAngle) * 2);

			// normalized arcStartDirection
			Vector2 arcStart = new Vector2(arcstartDirection.x / arcRadius, arcstartDirection.y / arcRadius);

			Handles.color = edgeColor;
			Handles.DrawSolidArc(from, Vector3.forward, arcStart, -arcAngle * Mathf.Rad2Deg, arcRadius);
			Handles.DrawSolidDisc(circleCenter, Vector3.forward, edgeWidth / 2);
			Handles.DrawLine(from, to);
			Handles.color = Color.white;
		}

	}
}
*/

