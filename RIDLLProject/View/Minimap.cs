using UnityEngine;
using UnityEditor;
using System.Linq;
using RelationsInspector.Extensions;
using System.Collections.Generic;

namespace RelationsInspector
{
	public enum MinimapLocation { TopLeft, TopRight, BottomLeft, BottomRight };

	internal class Minimap
	{
		internal static Rect GetRect( MinimapStyle mmStyle, MinimapLocation location, Rect contextRect )
		{
			int width, height;
			width = height = mmStyle.size;
			int spacing = mmStyle.spacing;
			switch ( location )
			{
				case MinimapLocation.TopLeft:
				default:
					return new Rect( spacing, spacing, width, height );

				case MinimapLocation.TopRight:
					return new Rect( contextRect.width - spacing - width, spacing, width, height );

				case MinimapLocation.BottomLeft:
					return new Rect( spacing, contextRect.height - spacing - height, width, height );

				case MinimapLocation.BottomRight:
					return new Rect( contextRect.width - spacing - width, contextRect.height - spacing - height, width, height );
			}
		}

		internal static Transform2d Draw( IEnumerable<Vector2> vertexPositions, Rect drawRect, Rect viewRect, bool showGraphBounds, MinimapStyle style )
		{
			// draw black outline
			EditorGUI.DrawRect( drawRect.AddBorder( 1f ), Color.black );
			// draw background
			EditorGUI.DrawRect( drawRect, style.backgroundColor );

			// fit vertex positions and viewRect into drawRect
			var pointsToFit = vertexPositions.Concat( 
				new[] {
					new Vector2( viewRect.xMin, viewRect.yMin ),
					new Vector2( viewRect.xMax, viewRect.yMax )
				} );

			var mmTransform = ViewUtil.FitPointsIntoRect( pointsToFit, drawRect.Scale( 0.9f ) );

			// draw view rect
			Rect tViewRect = mmTransform.Apply( viewRect ).Intersection( drawRect );
			Util.DrawRectOutline( tViewRect, style.viewRectColor );

			// draw vertex positions
			foreach ( var pos in vertexPositions )
				EditorGUI.DrawRect( Util.CenterRect( mmTransform.Apply( pos ), 2, 2 ), style.vertexMarkerColor );

			return mmTransform;
		}
	}
}
