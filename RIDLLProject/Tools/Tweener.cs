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
		//static List<TweenCollection> tweenCollections = new List<TweenCollection>();
		public static TweenCollection gen = new TweenCollection();	// generic collection

		static Tweener()
		{
			EditorApplication.update += Update;
			//tweenCollections.Add(gen);
		}

		static void Update()
		{
			//UpdateCollections( EditorApplication.timeSinceStartup );
			gen.Update(EditorApplication.timeSinceStartup);
		}
		/*
		static void UpdateCollections(double currentTime)
		{
			foreach (var collection in tweenCollections)
				collection.Update(currentTime);

			var expired = tweenCollections
				.Where(c => c.IsExpired(currentTime) && c != gen );

			if (expired.Any())
				foreach (var exp in expired.ToArray())
					tweenCollections.Remove(exp);
		}

		public static TweenCollection GetCollection()
		{
			var c = new TweenCollection();
			tweenCollections.Add(c);
			return c;
		}*/
	}

	public interface ITween
	{
		void Update(double time);
		bool IsExpired(double time);
		void Stack(ITween other);
	}

	public class TweenCollection
	{
		Dictionary<object, ITween> objTweens = new Dictionary<object, ITween>();

		public void Update()
		{
			Update(EditorApplication.timeSinceStartup); 
		}
		
		public void Update(double time)
		{
			foreach (var tween in objTweens.Values)
				tween.Update(time);

			var expired = objTweens
				.Where(t => t.Value.IsExpired(time))
				.Select(pair => pair.Key);

			if (expired.Any())
				foreach (var exp in expired.ToArray())
					objTweens.Remove(exp);
		}

		public bool IsExpired() 
		{ 
			return IsExpired(EditorApplication.timeSinceStartup); 
		}

		public bool IsExpired(double time)
		{
			return !objTweens.Any() || !objTweens.Values.Any( t => !t.IsExpired(time) );
		}

		public ITween MoveVertexTo<T, P>
			(
			VertexData<T, P> vertexData,
			Vector2 targetPosition,
			float duration,
			TweenCollisionHandling collisionHandling = TweenCollisionHandling.Replace
			) where T : class
		{
			var tween = new VertexPosTween<T, P>(vertexData, targetPosition, duration);

			if (objTweens.ContainsKey(vertexData))
			{
				switch (collisionHandling)
				{
					case TweenCollisionHandling.Replace:
						objTweens[vertexData] = tween;
						break;
					case TweenCollisionHandling.Stack:
						objTweens[vertexData].Stack(tween);
						break;
					case TweenCollisionHandling.Drop:
					default:
						break;
				}
			}
			else
			{
				objTweens[vertexData] = tween;
			}

			return objTweens[vertexData];
		}

		public ITween MoveTransform2dTo
			(
			Transform2d transform,
			Func<Transform2d, Transform2d> getEndValue,
			float duration,
			TweenCollisionHandling collisionHandling = TweenCollisionHandling.Replace
			)
		{
			var tween = new Transform2DTween(transform, getEndValue, duration);

			if (objTweens.ContainsKey(transform))
			{
				switch (collisionHandling)
				{
					case TweenCollisionHandling.Replace:
						objTweens[transform] = tween;
						break;
					case TweenCollisionHandling.Stack:
						objTweens[transform].Stack(tween);
						break;
					case TweenCollisionHandling.Drop:
					default:
						break;
				}
			}
			else
			{
				objTweens[transform] = tween;
			}

			return objTweens[transform];
		}

		public ITween MoveRectTo(RectObj rectObj, Vector2 targetPosition, float duration)
		{
			objTweens[rectObj] = new RectObjTween(rectObj, targetPosition, duration);
			return objTweens[rectObj];
		}

		public void Clear()
		{
			objTweens.Clear();
		}
	}

	public enum TweenCollisionHandling{ Stack, Replace, Drop };

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

		public void Stack(ITween other)
		{
			throw new System.NotImplementedException();
		}
	}

	public class Transform2DTween : ITween
	{
		Transform2d transform;
		Transform2d startValue;
		Transform2d endValue;
		Func<Transform2d, Transform2d> getEndValue;

		double startTime;
		double endTime;
		float duration;

		public Transform2DTween(
			Transform2d transform, 
			Func<Transform2d, Transform2d> getEndValue, 
			float duration)
		{
			this.transform = transform;
			this.startValue = new Transform2d(transform);
			this.endValue = getEndValue(startValue);
			this.getEndValue = getEndValue;
			//this.direction = (endPosition - startPosition).normalized;
			//this.distance = Vector2.Distance(startPosition, endPosition);
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

			transform.translation = startValue.translation + (endValue.translation - startValue.translation) * durationFraction;
			transform.scale = startValue.scale + (endValue.scale - startValue.scale) * durationFraction;
			transform.rotation = startValue.rotation + (endValue.rotation - startValue.rotation) * durationFraction;
		}

		public void Stack(ITween other)
		{
			var otherAsT = other as Transform2DTween;
			if (otherAsT == null)
				throw new System.ArgumentException("other");

			startTime = otherAsT.startTime;
			duration = otherAsT.duration;
			endTime = otherAsT.endTime;
			startValue = transform;
			endValue = otherAsT.getEndValue(endValue);
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

		public void Stack(ITween other)
		{
			throw new System.NotImplementedException();
		}
	}
}
