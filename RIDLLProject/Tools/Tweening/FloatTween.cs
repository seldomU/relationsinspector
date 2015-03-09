using UnityEngine;
using System;

namespace RelationsInspector.Tween
{
	public interface IFloatInterpolation
	{
		float Eval(float time);
	}

	public class TwoValueInterpolation : IFloatInterpolation
	{
		float a, b;
		Func<float, float, float, float> func;

		public TwoValueInterpolation(float a, float b, Func<float, float, float, float> func)
		{
			this.a = a;
			this.b = b;
			this.func = func;
		}

		public float Eval(float t)
		{
			return func(a, b, t);
		}
	}

	public class ThreeValueInterpolation : IFloatInterpolation
	{
		float a, b, c;
		Func<float, float, float, float, float> func;

		public ThreeValueInterpolation(float a, float b, float c, Func<float, float, float, float, float> func)
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
	}

	public class FloatTween
	{
		float startTime, duration;
		IFloatInterpolation interpolation;

		public FloatTween(float startTime, float duration, IFloatInterpolation interpolation)
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
		FloatTween xTween, yTween;
		Vector2 current;

		public Vector2Tween(Vector2 startValue, float startTime, Vector2 endValue, float endTime)
		{
			var xInterpolation = new TwoValueInterpolation(startValue.x, endValue.x, TweenUtil.Linear);
			var yInterpolation = new TwoValueInterpolation(startValue.y, endValue.y, TweenUtil.Linear);
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
