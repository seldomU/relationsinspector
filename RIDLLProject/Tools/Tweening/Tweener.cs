using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RelationsInspector.Tween
{
	public interface ITween
	{
		void Update(float time);
		bool IsExpired(float time);
	}
	
	public static class Tweener
	{
		public static TweenCollection gen = new TweenCollection();	// generic collection

		static Tweener()
		{
			EditorApplication.update += Update;
		}

		static void Update()
		{
			gen.Update((float)EditorApplication.timeSinceStartup);
		}
	}

	public enum TweenCollisionHandling { Stack, Replace, Drop };

	/*
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
			return time > this.endTime;
		}

		public void Update(double time)
		{
			double deltaTime = (time-startTime);
			float durationFraction = (float)deltaTime / duration;
			durationFraction = Mathf.Clamp01(durationFraction);
			var center = startPosition + direction * durationFraction * distance;
			rectObj.rect.center = center;
		}

		public void Stack(ITween other)
		{
			throw new System.NotImplementedException();
		}
	}*/
}
