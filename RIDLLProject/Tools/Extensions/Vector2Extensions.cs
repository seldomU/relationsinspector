using UnityEngine;

namespace RelationsInspector.Extensions
{
	public static class Vector2Extensions
	{
		public static Vector2 Clamp( this Vector2 source, Rect bounds )
		{
			return new Vector2
				(
				Mathf.Clamp( source.x, bounds.xMin, bounds.xMax ),
				Mathf.Clamp( source.y, bounds.yMin, bounds.yMax )
				);
		}

		public static Vector2 Clamp( this Vector2 source, Vector2 min, Vector2 max )
		{
			return new Vector2
				(
				Mathf.Clamp( source.x, min.x, max.x ),
				Mathf.Clamp( source.y, min.y, max.y )
				);
		}

		public static Vector2 RotateAround( this Vector2 source, Vector2 pivot, float angle )
		{
			source -= pivot;
			float radius = source.magnitude;
			angle += Util.GetAngle( source );
			source = new Vector2( Mathf.Cos( angle ) * radius, Mathf.Sin( angle ) * radius );

			return source + pivot;
		}

		// returns the left normal vector
		public static Vector2 Normal( this Vector2 source )
		{
			return new Vector2( -source.y, source.x );
		}
	}
}
