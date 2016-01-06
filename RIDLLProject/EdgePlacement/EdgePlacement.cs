using UnityEngine;

namespace RelationsInspector
{
	public delegate EdgePlacement EdgePlacementProvider( Rect sourceRect, Rect targetRect, float gapSize );

	public struct EdgePlacement
	{
		public Vector2 startPos;
		public Vector2 endPos;
		public Vector2 startNormal;
		public Vector2 endNormal;

		public EdgePlacement( Vector2 startPos, Vector2 endPos, Vector2 startNormal, Vector2 endNormal )
		{
			this.startPos = startPos;
			this.endPos = endPos;
			this.startNormal = startNormal;
			this.endNormal = endNormal;
		}

		public static EdgePlacement zero = new EdgePlacement( Vector2.zero, Vector2.zero, Vector2.one, Vector2.one );

		public EdgePlacement Swap()
		{
			return new EdgePlacement( endPos, startPos, endNormal, startNormal );
		}
	}
}

