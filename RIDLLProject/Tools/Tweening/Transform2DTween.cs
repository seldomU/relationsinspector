using UnityEngine;
using UnityEditor;

namespace RelationsInspector.Tween
{
	public class Transform2DTween : ITween
	{
		Transform2d obj;

		Vector2Tween scaleTween, translationTween;
		FloatTween rotationTween;

		float endTime;

		public Transform2d endValue { get; private set; }

		public delegate Transform2d GetEndValue(Transform2d start);

		public Transform2DTween(Transform2d transform, Transform2d endValue, float duration)
		{
			this.obj = transform;
			this.endValue = endValue;

			float time = (float)EditorApplication.timeSinceStartup;
			endTime = time + duration;

			scaleTween = new Vector2Tween(transform.scale, endValue.scale, time, endTime, TweenUtil.Linear);
			translationTween = new Vector2Tween(transform.translation, endValue.translation, time, endTime, TweenUtil.Linear);
			rotationTween = new FloatTween(time, endTime, new TwoValueEasing(transform.rotation, endValue.rotation, TweenUtil.Linear));
		}

		public Transform2DTween(Transform2d transform, Transform2DTween predecessor, Transform2d endValue, float duration)
		{
			this.obj = transform;
			this.endValue = endValue;

			float time = (float)EditorApplication.timeSinceStartup;
			endTime = time + duration;

			scaleTween = new Vector2Tween(
				transform.scale,
				predecessor.scaleTween.endValue,
				endValue.scale,
				time,
				endTime,
				TweenUtil.BezierQuadratic);

			translationTween = new Vector2Tween(
				transform.translation,
				predecessor.translationTween.endValue,
				endValue.translation,
				time,
				endTime,
				TweenUtil.BezierQuadratic);

			var rotationEasing = new ThreeValueEasing(
				transform.rotation,
				predecessor.rotationTween.easing.GetEndValue(),
				endValue.rotation,
				TweenUtil.BezierQuadratic);
			rotationTween = new FloatTween(time, duration, rotationEasing);
		}

		public void Update(float time)
		{
			obj.translation = translationTween.GetUpdated(time);
			obj.scale =	scaleTween.GetUpdated(time);
			obj.rotation = rotationTween.GetUpdated(time);
		}

		public bool IsExpired(float time)
		{
			return time > endTime;
		}

		public void Finish()
		{
			Update(endTime);
		}
	}
}
