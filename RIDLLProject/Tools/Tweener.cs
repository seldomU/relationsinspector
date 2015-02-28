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
		static Dictionary<object, ITween> objTweens = new Dictionary<object, ITween>();

		static Tweener()
		{
			EditorApplication.update += Update;
		}

		internal static bool IsActive()
		{
			return objTweens.Any();
		}

		static void Update()
		{
			UpdateTweens( EditorApplication.timeSinceStartup );
		}

		static void UpdateTweens(double currentTime)
		{
			foreach(var tween in objTweens.Values)
				tween.Update(currentTime);

			var expired = objTweens
				.Where(t => t.Value.IsExpired(currentTime))
				.Select(pair => pair.Key)
				.ToArray();

			foreach (var exp in expired)
				objTweens.Remove(exp);
		}

		public static void MoveVertexTo<T, P>(VertexData<T, P> vertexData, Vector2 targetPosition, float duration) where T : class
		{
			objTweens[vertexData] = new VertexPosTween<T, P>(vertexData, targetPosition, duration);
		}

		public static void MoveRectTo(RectObj rectObj, Vector2 targetPosition, float duration)
		{
			objTweens[rectObj] = new RectObjTween(rectObj, targetPosition, duration);
		}

		public static void Clear()
		{
			objTweens.Clear();
		}
	}

	public interface ITween
	{
		void Update(double time);
		bool IsExpired(double time);
	}

	public class VertexPosTween<T,P> : ITween where T : class
	{
		public VertexData<T, P> vertexData;
		Vector2 startPosition;
		Vector2 endPosition;
		Vector2 direction;
		float distance;
		double startTime;
		double endTime;
		float duration;

		public VertexPosTween(VertexData<T, P> vData, Vector2 targetPosition, float duration)
		{
			this.vertexData = vData;
			this.startPosition = vData.pos;
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
			double deltaTime = (time - startTime);
			float durationFraction = (float)deltaTime / duration;
			durationFraction = Mathf.Clamp01(durationFraction);
			var pos = startPosition + direction * durationFraction * distance;
			vertexData.pos = pos;
		}
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
	}
}
