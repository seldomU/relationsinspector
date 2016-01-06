using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	public class BasicRelationDrawer<T, P> : IRelationDrawer<T, P> where T : class
	{
		const float MinCircleRadius = 12f;

		public virtual Dictionary<Relation<T, P>, Rect> DrawRelation( IEnumerable<Relation<T, P>> toEdges, IEnumerable<Relation<T, P>> fromEdges, EdgePlacement placement, bool isSelfEdge, bool highlight, bool includeMarker, RelationDrawerStyle style, System.Func<P, Color> GetMarkerColor )
		{
			if ( isSelfEdge )
				return DrawSelfRelation( toEdges, fromEdges, placement, highlight, includeMarker, style, GetMarkerColor );
			else
				return DrawRegularRelation( toEdges, fromEdges, placement, highlight, includeMarker, style, GetMarkerColor );
		}

		public void DrawPseudoRelation( EdgePlacement placement, bool isSelfEdge, RelationDrawerStyle style )
		{
			if ( isSelfEdge )
			{
				float circleRadius = MinCircleRadius;
				Vector2 circleCenter = GetCircleCenter( placement.endPos, placement.startPos, circleRadius );
				Handles.color = style.highlightEdgeColor;
				Handles.DrawWireDisc( circleCenter, Vector3.forward, circleRadius );
				Handles.color = Color.white;
			}
			else
			{
				Handles.color = style.highlightEdgeColor;
				int lineWidth = style.highlightEdgeWidth;
				Handles.DrawAAPolyLine( lineWidth, placement.startPos, placement.endPos );
				Handles.color = Color.white;
			}
		}

		protected virtual Dictionary<Relation<T, P>, Rect> DrawRegularRelation( IEnumerable<Relation<T, P>> toEdges, IEnumerable<Relation<T, P>> fromEdges, EdgePlacement placement, bool highlight, bool includeMarker, RelationDrawerStyle style, System.Func<P, Color> GetMarkerColor )
		{
			Handles.color = highlight ? style.highlightEdgeColor : style.regularEdgeColor;
			int lineWidth = highlight ? style.highlightEdgeWidth : style.regularEdgeWidth;
			Handles.DrawAAPolyLine( lineWidth, placement.startPos, placement.endPos );
			Handles.color = Color.white;

			if ( includeMarker )
				return DrawMarkers( toEdges, fromEdges, placement.startPos, placement.endPos, style.markerSize, style.markerImage, GetMarkerColor );

			return new Dictionary<Relation<T, P>, Rect>();
		}

		static Dictionary<Relation<T, P>, Rect> DrawMarkers( IEnumerable<Relation<T, P>> toEdges, IEnumerable<Relation<T, P>> fromEdges, Vector2 startPos, Vector2 endPos, Vector2 size, Texture2D image, System.Func<P, Color> GetMarkerColor )
		{
			var edgeCenter = ( startPos + endPos ) * 0.5f;

			var toMarkers = DrawMarkers( toEdges, edgeCenter, startPos, size, image, GetMarkerColor );
			var fromMarkers = DrawMarkers( fromEdges, edgeCenter, endPos, size, image, GetMarkerColor );

			toMarkers.UnionWith( fromMarkers );
			return toMarkers;
		}

		static Dictionary<Relation<T, P>, Rect> DrawMarkers( IEnumerable<Relation<T, P>> edges, Vector2 startPos, Vector2 endPos, Vector2 size, Texture2D image, System.Func<P, Color> GetMarkerColor )
		{
			if ( !edges.Any() )
				return new Dictionary<Relation<T, P>, Rect>();

			var markerRects = new Dictionary<Relation<T, P>, Rect>();

			var edgeArray = edges.ToArray();
			int numMarkers = edgeArray.Length;

			float segmentSize = ( endPos - startPos ).magnitude / numMarkers;

			for ( int i = 0; i < edgeArray.Length; i++ )
			{
				float position = ( i + 0.5f ) * segmentSize;
				var edge = edgeArray[ i ];
				markerRects[ edge ] = DrawMarker( edge.Tag, startPos, endPos, size, image, GetMarkerColor( edge.Tag ), position );
			}

			return markerRects;
		}


		static Rect DrawMarker( P value, Vector2 startPos, Vector2 endPos, Vector2 size, Texture2D image, Color markerColor, float position = 0.75f )
		{
			Vector2 markerCenter = startPos + ( endPos - startPos ).normalized * position;
			float rotation = Util.GetAngle( endPos - startPos );
			var markerRect = Util.CenterRect( markerCenter, size );
			RelationDrawUtil.DrawRotatedTexture( image, markerCenter, size, rotation, markerColor );
			return markerRect;
		}


		protected virtual Dictionary<Relation<T, P>, Rect> DrawSelfRelation( IEnumerable<Relation<T, P>> toEdges, IEnumerable<Relation<T, P>> fromEdges, EdgePlacement placement, bool highlight, bool includeMarker, RelationDrawerStyle style, System.Func<P, Color> GetMarkerColor )
		{
			var edges = toEdges;    // self edges are stored in both: toEdges and fromEdges. so we pass only one set.
			float markerArcLength = Mathf.PI * 1.5f;
			float circleRadius = GetRadiusForFittingCircle( edges.Count(), markerArcLength, style.markerSize.x, style.markerSize.x / 4 );
			circleRadius = Mathf.Max( circleRadius, MinCircleRadius );
			Vector2 circleCenter = GetCircleCenter( placement.startPos, placement.endPos, circleRadius );

			Handles.color = highlight ? style.highlightEdgeColor : style.regularEdgeColor;
			Handles.DrawWireDisc( circleCenter, Vector3.forward, circleRadius );
			Handles.color = Color.white;

			if ( includeMarker )
			{
				float startAngle = -Mathf.PI * 1.5f;
				return DrawSelfMarkers( toEdges, circleCenter, circleRadius, startAngle, markerArcLength, style.markerSize, style.markerImage, GetMarkerColor );
			}
			return new Dictionary<Relation<T, P>, Rect>();
		}

		static float GetRadiusForFittingCircle( int numMarkers, float unitCircleArcLength, float markersize, float spacing )
		{
			float markerSpace = numMarkers * ( markersize + spacing ) + spacing;    // one more spacing than markers
			return markerSpace / unitCircleArcLength;   // * unitCircleRadius, which is 1
		}

		// find a circle of the given radius that goes through a and b
		// on the left side of the vector a->b
		static Vector2 GetCircleCenter( Vector2 a, Vector2 b, float radius )
		{
			var midPoint = ( a + b ) / 2f;
			var atob = b - a;
			var centerLineDirection = atob.Normal().normalized; // Normal() gives the left normal vector

			float leg = atob.magnitude / 2;
			float hypothenuse = radius;
			// make sure that hypothenuse > leg
			if ( hypothenuse <= leg )
				hypothenuse = leg * 1.5f;
			var distFromMidPoint = Mathf.Sqrt( hypothenuse * hypothenuse - leg * leg );
			return midPoint + centerLineDirection * distFromMidPoint;
		}

		static Dictionary<Relation<T, P>, Rect> DrawSelfMarkers( IEnumerable<Relation<T, P>> edges, Vector2 circleCenter, float radius, float startAngle, float arcLength, Vector2 markerSize, Texture2D image, System.Func<P, Color> GetMarkerColor )
		{
			var markerDict = new Dictionary<Relation<T, P>, Rect>();
			if ( !edges.Any() )
				return markerDict;

			var edgeArray = edges.ToArray();
			int numEdges = edgeArray.Length;
			float arcSegmentSize = arcLength / numEdges;

			for ( int i = 0; i < numEdges; i++ )
			{
				float angle = startAngle + ( i + 0.5f ) * arcSegmentSize;
				var position = Util.GetPositionOnCircle( angle ) * radius + circleCenter;
				var rotation = angle + Mathf.PI / 2;    // circle tangent
				RelationDrawUtil.DrawRotatedTexture( image, position, markerSize, rotation, GetMarkerColor( edgeArray[ i ].Tag ) );
				var edge = edgeArray[ i ];
				markerDict[ edge ] = Util.CenterRect( position, markerSize );
			}
			return markerDict;
		}
	}
}
