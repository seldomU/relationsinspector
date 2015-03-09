using UnityEngine;
using UnityEditor;

namespace RelationsInspector.Tween
{
	public class VertexPosTween<T, P> : ITween where T : class
	{
		VertexData<T, P> obj;
		Vector2Tween posTween;
		float endTime;

		public VertexPosTween(VertexData<T, P> vData, Vector2 targetPosition, float duration)
		{
			float time = (float)EditorApplication.timeSinceStartup;
			endTime = time + duration;

			this.obj = vData;
			this.posTween = new Vector2Tween(vData.pos, targetPosition, time, endTime, TweenUtil.Linear);
		}

		public VertexPosTween(VertexData<T, P> vData, VertexPosTween<T, P> predecessor, Vector2 targetPosition, float duration)
		{
			float time = (float)EditorApplication.timeSinceStartup;
			endTime = time + duration;

			this.obj = vData;
			this.posTween = new Vector2Tween(
				vData.pos,
				predecessor.posTween.endValue,
				targetPosition, 
				time, 
				endTime, 
				TweenUtil.BezierQuadratic);
		}

		public void Update(float time)
		{
			obj.pos = posTween.GetUpdated(time);
		}

		public bool IsExpired(float time)
		{
			return time > endTime;
		}
	}
}
