using System;

namespace RelationsInspector.Tweening
{
    public interface ITween
    {
        void Update( float time );
        bool IsExpired( float time );
        void Finish();
    }

    public class Tween<T> : ITween
    {
        Action<T> setValue;
        EvalAtIntervalTime<T> evalAtTime;
        float endTime;

        public Tween( Action<T> setValue, float duration, EvalAtNormTime<T> evalAtNormTime )
        {
            this.setValue = setValue;

            float startTime = (float) UnityEditor.EditorApplication.timeSinceStartup;
            this.endTime = startTime + duration;

            evalAtTime = time => evalAtNormTime( Clamp01( ( time - startTime ) / duration ) );
        }

        static float Clamp01(float f)
        {
            if (f < 0) return 0;
            if (f > 1) return 1;
            return f;
        }

        public void Update( float time )
        {
            setValue( evalAtTime( time ) );
        }

        public bool IsExpired( float time )
        {
            return time > endTime;
        }

        public void Finish()
        {
            Update( endTime );
        }

        public T GetFinalValue()
        {
            return evalAtTime(float.MaxValue);
        }
    }
}
