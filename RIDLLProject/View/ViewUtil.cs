using UnityEngine;
using System.Collections.Generic;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public static class ViewUtil
	{
		// get a transform that makes the graph fit into the view rect
		public static Transform2d FitPointsIntoRect( IEnumerable<Vector2> logicalPositions, Rect displayRect )
		{
			// get bounds, ensure they are non-zero
			Rect logicalPosBounds = Util.GetBounds( logicalPositions ).ClampExtents( 0.01f, 0.01f, float.MaxValue, float.MaxValue );

			var logicalExtents = logicalPosBounds.GetExtents();
			var displayExtents = displayRect.GetExtents();

			// fit logical extents into display extents
			var scale = new Vector2(
				logicalExtents.x == 0 ? 0 : displayExtents.x / logicalExtents.x,
				logicalExtents.y == 0 ? 0 : displayExtents.y / logicalExtents.y
				);

			// translation: put points center into outer rect center
			Vector2 pointsCenter = new Vector2( logicalPosBounds.center.x * scale.x, logicalPosBounds.center.y * scale.y );
			Vector2 translation = displayRect.center - pointsCenter;

			var transform = new Transform2d( translation, scale, 0 );
			return transform;
		}
	}
}
