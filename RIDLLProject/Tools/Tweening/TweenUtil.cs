using System;
using UnityEngine;

namespace RelationsInspector.Tweening
{
    public delegate float Easing2( float a, float b, float time );
    public delegate float Easing3( float a, float b, float c, float time );
    public delegate T EvalAtNormTime<T>( float time );  // normalized time (0..1)
    public delegate T EvalAtIntervalTime<T>( float time );

    public static class TweenUtil
	{
        public static EvalAtNormTime<float> GetTwoValueEasing( float a, float b, Easing2 easing)
        {
            return time => easing( a, b, time );
        }

        public static EvalAtNormTime<float> GetThreeValueEasing( float a, float b, float c, Easing3 easing )
        {
            return time => easing( a, b, c, time );
        }

        public static EvalAtNormTime<float> Float2( float startValue, float endValue, TwoValueEasing mode )
        {
            switch ( mode )
            {
                case TwoValueEasing.Linear:
                default:
                    return time => Easing.Linear( startValue, endValue, time );
            }
        }

        public static EvalAtNormTime<float> Float3( float startValue, float controlValue, float endValue, ThreeValueEasing mode )
        {
            switch ( mode )
            {
                case ThreeValueEasing.BezierQuadratic:
                default:
                    return time => Easing.BezierQuadratic( startValue, controlValue, endValue, time );
            }
        }

        public static EvalAtNormTime<Vector2> Vector2_2( Vector2 startValue, Vector2 endValue, TwoValueEasing mode )
        {
            var xEval = Float2( startValue.x, endValue.x, mode );
            var yEval = Float2( startValue.y, endValue.y, mode );
            return time => new Vector2( xEval( time ), yEval( time ) );
        }

        public static EvalAtNormTime<Vector2> Vector2_3( Vector2 startValue, Vector2 controlValue, Vector2 endValue, ThreeValueEasing mode )
        {
            var xEval = Float3( startValue.x, controlValue.x, endValue.x, mode );
            var yEval = Float3( startValue.y, controlValue.y, endValue.y, mode );
            return time => new Vector2( xEval( time ), yEval( time ) );
        }

        public static EvalAtNormTime<Transform2d> Transform2( Transform2d startValue, Transform2d endValue, TwoValueEasing mode )
        {
            var translateEval = Vector2_2( startValue.translation, endValue.translation, mode );
            var scaleEval = Vector2_2( startValue.scale, endValue.scale, mode );
            var rotateEval = Float2( startValue.rotation, endValue.rotation, mode );
            return time => new Transform2d( translateEval( time ), scaleEval( time ), rotateEval( time ) );
        }
    }
}
