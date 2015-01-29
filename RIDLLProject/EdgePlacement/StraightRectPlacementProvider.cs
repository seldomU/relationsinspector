using UnityEngine;
using System.Collections;

namespace RelationsInspector
{
	public class StraightRectPlacementProvider : IEdgePlacementProvider
	{
		static float selfEdgeOffset = 10;	// edge anchor distances from rect corner

		public EdgePlacement GetEdgePlacement(Rect sourceRect, Rect targetRect, float gapSize)
		{
			if (sourceRect == targetRect)
				return GetSelfEdgePlacement(sourceRect);

			var sourceToTarget = (targetRect.center - sourceRect.center).normalized;
			Tuple<Vector2, Vector2> start = GetExitPoint(sourceRect, sourceToTarget, gapSize);
			Tuple<Vector2, Vector2> end = GetExitPoint(targetRect, -sourceToTarget, gapSize);
			return new EdgePlacement(start._1, end._1, start._2, end._2);
		}

		static Tuple<Vector2, Vector2> GetExitPoint(Rect rect, Vector2 direction, float gapSize)
		{
			float xScale = Mathf.Abs(rect.width / 2 / direction.x);
			float yScale = Mathf.Abs(rect.height / 2 / direction.y);
			bool useXscale = xScale < yScale;
			Vector2 normal = useXscale ? Vector2.right : Vector2.up;
			normal *= (useXscale ? direction.x > 0 : direction.y > 0) ? 1 : -1;
			Vector2 exitPoint = rect.center + direction * Mathf.Min(xScale, yScale);
			exitPoint += direction * gapSize;
			return new Tuple<Vector2, Vector2>(exitPoint, normal);
		}

		static EdgePlacement GetSelfEdgePlacement(Rect rect)
		{
			Vector2 start = new Vector2(rect.xMin, rect.yMin + selfEdgeOffset);
			Vector2 end = new Vector2(rect.xMin + selfEdgeOffset, rect.yMin);
			return new EdgePlacement(start, end, -Vector2.right, -Vector2.up);
		}
	}
}
