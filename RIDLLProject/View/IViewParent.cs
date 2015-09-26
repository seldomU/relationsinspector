using UnityEngine;
using System.Collections;
using System;

namespace RelationsInspector
{
	internal interface IViewParent<T, P> where T : class
	{
		void RepaintView();
		Rect GetViewRect();

		void MoveEntity(T entity, Vector2 delta);

		bool IsRoot(T entity);  // is entity derived from a inspector target object?
        IGraphBackendInternal<T, P> GetBackend();
	}
}
