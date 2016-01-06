using UnityEngine;

namespace RelationsInspector.Extensions
{
	public static class RectExtensions
	{
		public static Vector2 GetOrigin( this Rect rect )
		{
			return new Vector2( rect.x, rect.y );
		}

		public static Rect ResetOrigin( this Rect rect )
		{
			return new Rect( 0, 0, rect.width, rect.height );
		}

		public static Vector2 GetExtents( this Rect rect )
		{
			return new Vector2( rect.width, rect.height );
		}

		public static Rect ClampExtents( this Rect rect, float minWidth, float minHeight, float maxWidth, float maxHeight )
		{
			float width = Mathf.Clamp( rect.width, minWidth, maxWidth );
			float height = Mathf.Clamp( rect.height, minHeight, maxHeight );
			return Util.CenterRect( rect.center, width, height );
		}

		public static Rect Scale( this Rect rect, float factor )
		{
			return Util.CenterRect( rect.center, rect.GetExtents() * factor );
		}

		public static Rect Scale( this Rect rect, Vector2 scale )
		{
			return Util.CenterRect( rect.center, rect.width * scale.x, rect.height * scale.y );
		}

		public static Rect Move( this Rect rect, Vector2 shift )
		{
			return new Rect
				(
				rect.xMin + shift.x,
				rect.yMin + shift.y,
				rect.width,
				rect.height
				);
		}

		public static Vector2[] Vertices( this Rect rect )
		{
			return new[]
			{
				new Vector2(rect.xMin, rect.yMin),
				new Vector2(rect.xMin, rect.yMax),
				new Vector2(rect.xMax, rect.yMax),
				new Vector2(rect.xMax, rect.yMin)
			};
		}

		public static Rect Intersection( this Rect rect, Rect other )
		{
			float xMin = Mathf.Max( rect.xMin, other.xMin );
			float xMax = Mathf.Min( rect.xMax, other.xMax );
			float yMin = Mathf.Max( rect.yMin, other.yMin );
			float yMax = Mathf.Min( rect.yMax, other.yMax );

			if ( xMin >= xMax || yMin >= yMax )
				return Util.rectZero;

			return Rect.MinMaxRect( xMin, yMin, xMax, yMax );
		}

		public static bool Intersects( this Rect a, Rect b )
		{
			return ( a.xMin <= b.xMax &&
					b.xMin <= a.xMax &&
					a.yMin <= b.yMax &&
					b.yMin <= a.yMax );
		}

		public static Rect AddBorder( this Rect rect, float border )
		{
			return new Rect
				(
				rect.xMin - border,
				rect.yMin - border,
				rect.width + 2 * border,
				rect.height + 2 * border
				);
		}

		public static Rect AddBorder( this Rect rect, float horizontal, float vertical )
		{
			return new Rect
				(
				rect.xMin - horizontal,
				rect.yMin - vertical,
				rect.width + 2 * horizontal,
				rect.height + 2 * vertical
				);
		}

		public static Rect AddBorder( this Rect rect, float left, float right, float top, float bottom )
		{
			return new Rect
				(
				rect.xMin - left,
				rect.yMin - top,
				rect.width + left + right,
				rect.height + top + bottom
				);
		}
	}
}
