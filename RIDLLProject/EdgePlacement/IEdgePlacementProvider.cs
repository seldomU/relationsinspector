using UnityEngine;
using System.Collections;

namespace RelationsInspector
{
	public interface IEdgePlacementProvider
	{
		EdgePlacement GetEdgePlacement(Rect sourceRect, Rect targetRect, float gapSize);
	}
}
