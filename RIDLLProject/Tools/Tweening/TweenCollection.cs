using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Tweening
{
    public class TweenCollection
	{
        HashSet<ITween> tweens = new HashSet<ITween>();
        Dictionary<object, ITween> tweenOwners = new Dictionary<object, ITween>();

        public bool HasChanges { get; private set; }    // true if the last update changed any tweened values

		public void Update()
		{
            Update( (float) EditorApplication.timeSinceStartup );
		}

		public void Update(float time)
		{
            // if the collection contains any tweens, there will be value changes
            // even the expired tweens still change values (setting the final ones)
            HasChanges = tweens.Any();

            // find the expired
            var expired = tweens.Where( t => t.IsExpired( time ) );

			// finalize and remove them
			if (expired.Any())
			{
				foreach (var exp in expired.ToArray())
				{
					exp.Finish();
					tweens.Remove(exp);
				}
			}

			// update the remaining
			foreach (var tween in tweens)
				tween.Update(time);
		}

		public void Clear()
		{
			tweens.Clear();
		}

        public void Add( ITween tween )
        {
            tweens.Add( tween );
        }

        public void Replace(object owner, ITween tween)
        {
            if (tweenOwners.ContainsKey(owner))
                tweens.Remove(tweenOwners[owner]);

            tweens.Add(tween);
            tweenOwners[owner] = tween;
        }

        public bool HasTween(object owner)
        {
            return tweenOwners.ContainsKey(owner);
        }

        public T GetFinalValue<T>(object owner)
        {
            ITween tween;
            if (!tweenOwners.TryGetValue(owner, out tween))
                return default(T);

            return (tween as Tween<T>).GetFinalValue();
        }
	}
}
