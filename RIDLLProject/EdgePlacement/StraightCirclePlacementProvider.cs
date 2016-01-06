using UnityEngine;

namespace RelationsInspector
{
	public class StraightCirclePlacementProvider
	{
		public static EdgePlacement GetEdgePlacement( Rect sourceRect, Rect targetRect, float gapSize )
		{
			if ( sourceRect == targetRect )
				return GetSelfEdgePlacement( sourceRect );

			var sourceCenter = sourceRect.center;
			var sourceRadius = sourceRect.width / 2;

			var targetCenter = targetRect.center;
			var targetRadius = targetRect.width / 2;

			var direction = ( targetCenter - sourceCenter ).normalized;

			var startPos = sourceCenter + direction * ( sourceRadius + gapSize );
			var endPos = targetCenter - direction * ( targetRadius + gapSize );
			return new EdgePlacement( startPos, endPos, Vector2.zero, Vector2.zero );
		}

		static EdgePlacement GetSelfEdgePlacement( Rect rect )
		{
			var startPos = new Vector2( rect.xMin, rect.center.y );
			var endPos = new Vector2( rect.center.x, rect.yMin );

			return new EdgePlacement( startPos, endPos, Vector2.zero, Vector2.zero );
		}
	}
}
