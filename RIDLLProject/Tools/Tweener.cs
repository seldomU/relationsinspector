using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RelationsInspector
{
	public static class Tweener
	{
		//static double lastUpdateTime;
		static List<ITween> tweens;

		static Tweener()
		{
			EditorApplication.update += Update;
			tweens = new List<ITween>();
		}

		static void Update()
		{
			double currentTime = EditorApplication.timeSinceStartup;
			//double deltaTime = currentTime - lastUpdateTime;
			//if (deltaTime > 0.5)
			//	deltaTime = 0;
			//lastUpdateTime = currentTime;

			UpdateTweens(currentTime);
		}

		static void UpdateTweens(double currentTime)
		{
			foreach (var tween in tweens)
			{
				tween.Update(currentTime);
			}

			tweens.RemoveAll( t => t.IsExpired(currentTime) );
		}

		public static void MoveRectTo(RectObj rectObj, Vector2 targetPosition, float duration)
		{
			tweens.Add(new RectObjTween(rectObj, targetPosition, duration));
		}
	}

	public interface ITween
	{
		void Update(double time);
		bool IsExpired(double time);
	}


	public class RectObj
	{
		public Rect rect;
		public RectObj(Rect rect) { this.rect = rect; }
	}

	public class RectObjTween : ITween
	{
		RectObj rectObj;
		Vector2 startPosition;
		Vector2 endPosition;
		Vector2 direction;
		float distance;
		double startTime;
		double endTime;
		float duration;

		public RectObjTween(RectObj rectObj, Vector2 targetPosition, float duration)
		{
			this.rectObj = rectObj;
			this.startPosition = rectObj.rect.center;
			this.endPosition = targetPosition;
			this.direction = (endPosition - startPosition).normalized;
			this.distance = Vector2.Distance(startPosition, endPosition);
			this.startTime = EditorApplication.timeSinceStartup;
			this.endTime = startTime + duration;
			this.duration = duration;
		}

		public bool IsExpired(double time)
		{
			bool expired = time > this.endTime;
			//if (expired)
			//	Debug.Log("time is up. " + time);
			return expired;
		}

		public void Update(double time)
		{
			double deltaTime = (time-startTime);
			float durationFraction = (float)deltaTime / duration;
			durationFraction = Mathf.Clamp01(durationFraction);
			var center = startPosition + direction * durationFraction * distance;
			rectObj.rect.center = center;
			//Debug.Log("update " + time);
		}
	}
}
