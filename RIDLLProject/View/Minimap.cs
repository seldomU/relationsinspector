using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using RelationsInspector.Extensions;
using System.Collections.Generic;

namespace RelationsInspector
{
    public enum MinimapLocation { TopLeft, TopRight, BottomLeft, BottomRight };

    internal class Minimap
	{
        const int verticalOffset = 16;  // the editor window draws this canvas in a layout group. that group adds space at the top

        internal static Rect GetRect(MinimapStyle mmStyle, MinimapLocation location, Rect contextRect)
        {
            int width, height;
            width = height = mmStyle.size;
            int spacing = mmStyle.spacing;
            switch ( location )
            {
                case MinimapLocation.TopLeft:
                default:
                    return new Rect(contextRect.x+spacing, contextRect.y + spacing - verticalOffset, width, height);

                case MinimapLocation.TopRight:
                    return new Rect( contextRect.xMax - spacing - width, contextRect.y + spacing - verticalOffset, width, height );

                case MinimapLocation.BottomLeft:
                    return new Rect(contextRect.x + spacing, contextRect.yMax - spacing - height - verticalOffset, width, height);

                case MinimapLocation.BottomRight:
                    return new Rect(contextRect.xMax - spacing - width, contextRect.yMax - spacing - height - verticalOffset, width, height);
            }
        }

		internal static Vector2 Draw(IEnumerable<Vector2> windowPositions, Rect drawRect, Rect graphViewRect, bool showGraphBounds, MinimapStyle style)
		{
			// draw black outline
			EditorGUI.DrawRect(drawRect.AddBorder(1f), Color.black);
			// draw background
			EditorGUI.DrawRect(drawRect, style.backgroundColor);

			// determine scale
			var graphVertsBounds = Util.GetBounds(windowPositions);
			Rect graphBounds = Util.GetBounds(graphVertsBounds, graphViewRect);
			var graphExtents = graphBounds.GetExtents();	//new Vector2(graphBounds.width, graphBounds.height);
			var drawRectExtents = drawRect.GetExtents();
			var xScale = drawRectExtents.x / graphExtents.x;
			var yScale = drawRectExtents.y / graphExtents.y;
			float graphToDrawScale = Mathf.Min(xScale, yScale) *0.8f;	// make it fit in both dimensions and leave a small margin to the draw area border

			var graphCenter = graphBounds.center;
			var drawRectCenter = drawRect.center;

			// draw view rect
			Rect graphCenterViewRect = Util.Transform(graphViewRect, -graphCenter, 1);
			Rect windowViewRect = Util.Transform(graphCenterViewRect, drawRectCenter, graphToDrawScale);
			windowViewRect = windowViewRect.Intersection(drawRect);
			if (windowViewRect != Util.rectZero)
				Util.DrawRectOutline(windowViewRect, style.viewRectColor);

			// draw vertices
			foreach (var graphPos in windowPositions)
			{
				Vector2 graphCenterPos = Util.Transform(graphPos, -graphCenter, 1);
				Vector2 windowPos = Util.Transform(graphCenterPos, drawRectCenter, graphToDrawScale);

				EditorGUI.DrawRect(Util.CenterRect(windowPos, 2, 2), style.vertexMarkerColor);
			}

			// debug: draw graph bounds
			if (showGraphBounds)
			{
				Rect trueGraphBounds = Util.GetBounds( windowPositions );
				Rect graphCenterBounds = Util.Transform(trueGraphBounds, -graphCenter, 1);
				Rect windowBounds = Util.Transform(graphCenterBounds, drawRectCenter, graphToDrawScale);
				Util.DrawRectOutline(windowBounds, Color.white);
			}

			// find new center: if there is a mousedown event in the rect, make that position the new view center
			Vector2 newCenter = graphViewRect.center;
			var ev = Event.current;
			switch (ev.type)
			{
				case EventType.mouseDown:
				//case EventType.mouseDrag:
					if (drawRect.Contains(ev.mousePosition))
					{
						var clickPosInGraphSpace =  Util.UnTransform(ev.mousePosition, drawRectCenter, graphToDrawScale);
						clickPosInGraphSpace = Util.UnTransform(clickPosInGraphSpace, -graphCenter, 1);
						newCenter = clickPosInGraphSpace;
						ev.Use();
					}
					break;
			}

			return newCenter;
		}

		static Vector2 GraphToDrawArea(Vector2 graphPoint, Vector2 graphCenter, float graphToDrawAreaScale, Vector2 drawAreaCenter)
		{
			// transform to graph center coordinates
			var graphLocal = graphPoint - graphCenter;

			// transform to draw area coordinates
			return Util.Transform(graphLocal, drawAreaCenter, graphToDrawAreaScale);
		}
	}
}
