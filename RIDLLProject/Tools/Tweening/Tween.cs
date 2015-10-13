using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            evalAtTime = time => evalAtNormTime( ( time - startTime ) / duration );
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
    }
}
