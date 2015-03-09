using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationsInspector.Tween
{
	public static class TweenUtil
	{
		// returns value at time t on the curve defined by a,b,c (with t in [0,1])
		public static float BezierQuadratic(float a, float b, float c, float t)
		{
			float r = 1 - t;	// remaining time
			return r * r * a + 2 * r * t * b + t * t * c;
		}

		// linear interpolation between a and b (with t in [0,1])
		public static float Linear(float a, float b, float t)
		{
			return (1 - t) * a + t * b;
		}
	}
}
