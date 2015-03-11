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
			{
				foreach (var exp in expired.ToArray())
				{
					objTweens[exp].Finish();
					objTweens.Remove(exp);
				}
			}

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
			bool merge
			) where T : class
		{
			if (merge && objTweens.ContainsKey(vertexData))
			{
				var predecessor = objTweens[vertexData] as VertexPosTween<T, P>;
				objTweens[vertexData] = new VertexPosTween<T, P>(vertexData, predecessor, targetPosition, duration);
			}
			else
			{
				objTweens[vertexData] = new VertexPosTween<T, P>(vertexData, targetPosition, duration);
			}
			return objTweens[vertexData];
		}

		public ITween MoveTransform2dTo
			(
			Transform2d transform,
			//Transform2d endValue,
			Transform2DTween.GetEndValue getEndValue,
			float duration,
			bool merge
			)
		{
			if (merge && objTweens.ContainsKey(transform))
			{
				var predecessor = objTweens[transform] as Transform2DTween;
				var endValue = getEndValue(predecessor.endValue);
				objTweens[transform] = new Transform2DTween(transform, predecessor, endValue, duration);
			}
			else
			{
				objTweens[transform] = new Transform2DTween(transform, getEndValue(transform), duration);
			}

			return objTweens[transform];
		}

		public ITween MoveRectTo(RectObj rectObj, Vector2 targetPosition, float duration, bool merge)
		{
			if (merge && objTweens.ContainsKey(rectObj))
			{
				var predecessor = objTweens[rectObj] as RectObjTween;
				objTweens[rectObj] = new RectObjTween(rectObj, predecessor, targetPosition, duration, TweenUtil.BezierQuadratic);
			}
			else
			{
				objTweens[rectObj] = new RectObjTween(rectObj, targetPosition, duration, TweenUtil.Linear);
			}
			return objTweens[rectObj];
		}
	}

}
