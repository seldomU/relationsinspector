using UnityEngine;

namespace RelationsInspector.Tweening
{
    public delegate float Easing2( float time );
    public delegate float Easing3( float a, float b, float c, float time );
    public delegate T EvalAtNormTime<T>( float time );  // expects normalized time (0..1)
    public delegate T EvalAtIntervalTime<T>( float time );

    public static class TweenUtil
	{

        public static EvalAtNormTime<float> Float2( float startValue, float endValue, TwoValueEasing mode )
        {
            float diff = endValue - startValue;
            return time => startValue + diff * Easing.twoValueEasings[mode]( time );
        }

        public static EvalAtNormTime<float> Float3( float startValue, float controlValue, float endValue, ThreeValueEasing mode )
        {
            return time => Easing.threeValueEasings[mode](startValue, controlValue, endValue, time);
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

        public static EvalAtNormTime<Vector2> GetCombinedEasing(object owner, TweenCollection collection, Vector2 startValue, Vector2 endValue)
        {
            if(!collection.HasTween(owner))
                return Vector2_2(startValue, endValue, TwoValueEasing.Linear);

            Vector2 midValue = collection.GetFinalValue<Vector2>(owner);
            return Vector2_3(startValue, midValue, endValue, ThreeValueEasing.BezierQuadratic);
        }
    }
}
