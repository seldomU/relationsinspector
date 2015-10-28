using System.Collections.Generic;
using UnityEngine;

namespace RelationsInspector.Tweening
{
    public enum TwoValueEasing
    {
        Linear,
        QuadraticIn,
        QuadraticOut,
        QuadraticInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        SineIn,
        SineOut,
        SineInOut
    }

    public enum ThreeValueEasing
    {
        BezierQuadratic
    }

    class Easing
    {
        public static readonly Dictionary<TwoValueEasing, Easing2> twoValueEasings = new Dictionary<TwoValueEasing,Easing2>
        {
            {TwoValueEasing.Linear, Linear },
            {TwoValueEasing.QuadraticIn, QuadraticIn  },
            {TwoValueEasing.QuadraticOut, QuadraticOut  },
            {TwoValueEasing.QuadraticInOut, QuadraticInOut },
            {TwoValueEasing.CubicIn, CubicIn },
            {TwoValueEasing.CubicOut, CubicOut },
            {TwoValueEasing.CubicInOut, CubicInOut },
            {TwoValueEasing.SineIn, SineIn },
            {TwoValueEasing.SineOut, SineOut },
            {TwoValueEasing.SineInOut, SineInOut },
        };

        public static readonly Dictionary<ThreeValueEasing, Easing3> threeValueEasings = new Dictionary<ThreeValueEasing,Easing3>
        {
            {ThreeValueEasing.BezierQuadratic, BezierQuadratic}
        };

        // returns value at time t on the curve defined by a,b,c (with t in [0,1])
        public static float BezierQuadratic( float a, float b, float c, float t )
        {
            float r = 1 - t;    // remaining time
            return r * r * a + 2 * r * t * b + t * t * c;
        }

        // linear interpolation between a and b (with t in [0,1])
        public static float Linear( float t )
        {
            return t;
        }

        public static float QuadraticIn(float t)
        {
            return t*t;
        }

        public static float QuadraticOut(float t)
        {
            return t*(2-t);
        }

        public static float QuadraticInOut(float t)
        {
            if ((t *= 2) < 1)
				return 0.5f * t * t;

			return - 0.5f * (--t * (t - 2) - 1);
        }

        public static float CubicIn(float t)
        {
            return t*t*t;
        }

        public static float CubicOut(float t)
        {
            return --t * t * t + 1; ;
        }

        public static float CubicInOut(float k)
        {
            if ((k *= 2) < 1)
            {
                return 0.5f * k * k * k;
            }

            return 0.5f * ((k -= 2) * k * k + 2);
        }

        
		public static float SineIn(float k)
        {
			return 1 - Mathf.Cos(k * Mathf.PI / 2);
		}

		public static float SineOut(float k)
        {
			return Mathf.Sin(k * Mathf.PI / 2);
		}

		public static float SineInOut(float k)
        {
			return 0.5f * (1 - Mathf.Cos(Mathf.PI * k));
		}

    }
}
