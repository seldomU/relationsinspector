using UnityEngine;
using System;

namespace RelationsInspector.Tween
{
	public class RectObj
	{
		public Rect rect;
		public RectObj(Rect rect) { this.rect = rect; }
	}

	public class RectObjTween : ITween
	{
		RectObj obj;
		Vector2Tween posTween;
		float endTime;

		// move from current to target position
		public RectObjTween(RectObj rectObj, Vector2 targetPosition, float duration, Easing2 func)
		{
			float time = (float)UnityEditor.EditorApplication.timeSinceStartup;
			endTime = time + duration;

			this.obj = rectObj;
			this.posTween = new Vector2Tween(rectObj.rect.center, targetPosition, time, endTime, func);
		}

		// move from current to target position, while taking the previous target into account (e.g. bezier)
		public RectObjTween(RectObj rectObj, RectObjTween predecessor, Vector2 targetPosition, float duration, Easing3 func)
		{
			float time = (float)UnityEditor.EditorApplication.timeSinceStartup;
			endTime = time + duration;

			this.obj = rectObj;
			var pos1 = rectObj.rect.center;
			var pos2 = predecessor.posTween.endValue;
			var pos3 = targetPosition;
			this.posTween = new Vector2Tween(pos1, pos2, pos3, time, endTime, func);
		}

		public void Update(float time)
		{
			obj.rect.center = posTween.GetUpdated(time);
		}

		public bool IsExpired(float time)
		{
			return time > endTime;
		}
	}

	public delegate float Easing2(float a, float b, float time);
	public delegate float Easing3(float a, float b, float c, float time);
	
	public interface IFloatEasing
	{
		float Eval(float time);
		float GetEndValue();
	}

	public class TwoValueEasing : IFloatEasing
	{
		float a, b;
		Easing2 func;

		public TwoValueEasing(float a, float b, Easing2 func)
		{
			this.a = a;
			this.b = b;
			this.func = func;
		}

		public float Eval(float t)
		{
			return func(a, b, t);
		}

		public float GetEndValue()
		{
			return b;
		}
	}

	public class ThreeValueEasing : IFloatEasing
	{
		float a, b, c;
		Easing3 func;

		public ThreeValueEasing(float a, float b, float c, Easing3 func)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.func = func;
		}

		public float Eval(float t)
		{
			return func(a, b, c, t);
		}

		public float GetEndValue()
		{
			return c;
		}
	}

	public class FloatTween
	{
		float startTime;
		float duration;
		IFloatEasing interpolation;

		public FloatTween(float startTime, float duration, IFloatEasing interpolation)
		{
			this.startTime = startTime;
			this.duration = duration;
			this.interpolation = interpolation;
		}

		public float GetUpdated(float time)
		{
			float t = (time - startTime) / duration;
			return interpolation.Eval(t);
		}

		/*public bool IsExpired(float time)
		{
			return time > startTime + duration;
		}*/
	}

	/*
	public class LinearFloatTween : ITween
	{
		float startTime, duration;
		TwoValueInterpolation interpolation;

		public LinearFloatTween(float startValue, float startTime, float endValue, float duration)
		{
			this.startTime = startTime;
			this.duration = duration;
			this.interpolation = new TwoValueInterpolation(startValue, endValue, TweenUtil.Linear);
		}

		public float Eval(float time)
		{
			float t = (time - startTime) / duration;
			return interpolation.Eval(t);
		}
	}*/

	public class Vector2Tween
	{
		FloatTween xTween;
		FloatTween yTween;
		public Vector2 endValue { get; private set; }

		public Vector2Tween(Vector2 startValue, Vector2 endValue, float startTime, float endTime, Easing2 func)
		{
			var xInterpolation = new TwoValueEasing(startValue.x, endValue.x, func);
			var yInterpolation = new TwoValueEasing(startValue.y, endValue.y, func);
			this.endValue = endValue;
            float duration = endTime - startTime;
			xTween = new FloatTween(startTime, duration, xInterpolation);
			yTween = new FloatTween(startTime, duration, yInterpolation);
		}

		public Vector2Tween(Vector2 pos1, Vector2 pos2, Vector2 pos3, float startTime, float endTime, Easing3 func)
		{
			var xInterpolation = new ThreeValueEasing(pos1.x, pos2.x, pos3.x, func);
			var yInterpolation = new ThreeValueEasing(pos1.y, pos2.y, pos3.y, func);
			this.endValue = pos3;
			float duration = endTime - startTime;
			xTween = new FloatTween(startTime, duration, xInterpolation);
			yTween = new FloatTween(startTime, duration, yInterpolation);
		}

		public Vector2 GetUpdated(float time)
		{
			return new Vector2( xTween.GetUpdated(time), yTween.GetUpdated(time) );
		}
	}
}
