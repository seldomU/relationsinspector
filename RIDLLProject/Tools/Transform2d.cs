using UnityEngine;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	[System.Serializable]
	public class Transform2d
	{
		public Vector2 translation;
		public Vector2 scale;
		public float rotation;

		public Transform2d() : this( Vector2.zero, Vector2.one, 0 ) { }

		public Transform2d( Vector2 translation, Vector2 scale, float rotation )
		{
			this.translation = translation;
			this.scale = scale;
			this.rotation = rotation;
		}

		public Transform2d( Transform2d other )
		{
			this.translation = other.translation;
			this.scale = other.scale;
			this.rotation = other.rotation;
		}

		public static void Copy( Transform2d from, Transform2d to )
		{
			to.translation = from.translation;
			to.scale = from.scale;
			to.rotation = from.rotation;
		}

		public Vector2 Apply( Vector2 source )
		{
			return new Vector2( source.x * scale.x + translation.x, source.y * scale.y + translation.y );
		}

		public Rect Apply( Rect source )
		{
			var newCenter = Apply( source.center );
			return Util.CenterRect( newCenter, ApplyScale( source.GetExtents() ) );
		}

		public Vector2 ApplyScale( Vector2 source )
		{
			return new Vector2( source.x * scale.x, source.y * scale.y );
		}

		public Rect ApplyScale( Rect r )
		{
			Vector2 center = r.center;
			Vector2 newCenter = new Vector2( center.x * scale.x, center.y * scale.y );
			return Util.CenterRect( newCenter, r.width * scale.x, r.height * scale.y );
		}

		public Vector2 Revert( Vector2 source )
		{
			float x = scale.x != 0 ? ( source.x - translation.x ) / scale.x : 0;
			float y = scale.y != 0 ? ( source.y - translation.y ) / scale.y : 0;
			return new Vector2( x, y );
		}

		public Rect Revert( Rect source )
		{
			Vector2 center = Revert( source.center );
			Vector2 extents = RevertScale( source.GetExtents() );
			return Util.CenterRect( center, extents );
		}

		public Vector2 RevertScale( Vector2 source )
		{
			float x = scale.x != 0 ? source.x / scale.x : 0;
			float y = scale.y != 0 ? source.y / scale.y : 0;
			return new Vector2( x, y );
		}

		/*
		private Vector2 ApplyRotation(Vector2 source, Vector2 pivot)
		{
			source -= pivot;

			float angle = Util.GetAngle(source) + rotation;
			float radius = source.magnitude;
			source = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

			return source + pivot;
		}
		*/

		public override string ToString()
		{
			return string.Format( "T {0} R {1} S {2}", translation, rotation, scale );
		}
	}
}
