using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Tween
{
	public class TweenCollection
	{
		Dictionary<object, ITween> objTweens = new Dictionary<object, ITween>();

		public void Update()
		{
			Update((float)EditorApplication.timeSinceStartup);
		}

		public void Update(float time)
		{
			// find the expired
			var expired = objTweens
				.Where(t => t.Value.IsExpired(time))
				.Select(pair => pair.Key);

			// remove them
			if (expired.Any())
				foreach (var exp in expired.ToArray())
					objTweens.Remove(exp);

			// update the remaining
			foreach (var tween in objTweens.Values)
				tween.Update(time);
		}

		public bool IsExpired()
		{
			return IsExpired((float)EditorApplication.timeSinceStartup);
		}

		public bool IsExpired(float time)
		{
			return !objTweens.Any() || !objTweens.Values.Any(t => !t.IsExpired(time));
		}

		public void Clear()
		{
			objTweens.Clear();
		}

		public ITween MoveVertexTo<T, P>
			(
			VertexData<T, P> vertexData,
			Vector2 targetPosition,
			float duration,
			TweenCollisionHandling collisionHandling = TweenCollisionHandling.Replace
			) where T : class
		{
			objTweens[vertexData] = new VertexPosTween<T, P>(vertexData, targetPosition, duration);
			return objTweens[vertexData];
			/*
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
			*/
		}

		public ITween MoveTransform2dTo
			(
			Transform2d transform,
			Transform2d endValue,
			float duration,
			TweenCollisionHandling collisionHandling = TweenCollisionHandling.Replace
			)
		{
			objTweens[transform] = new Transform2DTween(transform, endValue, duration);
			return objTweens[transform];

			/*
			if (objTweens.ContainsKey(transform))
			{
				switch (collisionHandling)
				{
					case TweenCollisionHandling.Replace:
						objTweens[transform] = new Transform2DTween(transform, endValue, duration);
						break;
					case TweenCollisionHandling.Stack:
						// copy the tween's current state to transform
						Transform2d.Copy((objTweens[transform] as Transform2DTween).GetCurrent(), transform);
						objTweens[transform] = new Transform2DTween(transform, endValue, duration);
						break;
					case TweenCollisionHandling.Drop:
					default:
						break;
				}
			}
			else
			{
				objTweens[transform] = new Transform2DTween(transform, endValue, duration);
			}

			return objTweens[transform];
			*/
		}


		/*public ITween MoveRectTo(RectObj rectObj, Vector2 targetPosition, float duration)
		{
			objTweens[rectObj] = new RectObjTween(rectObj, targetPosition, duration);
			return objTweens[rectObj];
		}*/
	}

}
