using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public static class ViewUtil
	{
		// get a transform that makes the graph fit into the view rect
		public static Transform2d FitPointsIntoRect(IEnumerable<Vector2> points,  Rect outerRect, float relativeMargin)
		{
			Rect pointsBounds = Util.GetBounds( points );
			// make sure the extents are non-zero
			pointsBounds = pointsBounds.ClampExtents(0.01f, 0.01f, float.MaxValue, float.MaxValue);

			// scale: fit points into outer rect
			float bestScale = Util.MinScale(pointsBounds.GetExtents(), outerRect.GetExtents());
			bestScale *= (1 - relativeMargin);	// leave some margin on the border
			Vector2 scale = new Vector2(bestScale, bestScale);

			// translation: put points center into outer rect center
			Vector2 pointsCenter = pointsBounds.center * bestScale;
			Vector2 translation = outerRect.center - pointsCenter;

			var transform = new Transform2d(translation, scale, 0);
			return transform;
		}
	}
}
