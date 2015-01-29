using UnityEngine;
using System.Collections;
using System;

namespace RelationsInspector
{
	internal interface IViewParent<T, P> where T : class
	{
		void RepaintView();
		Rect GetViewRect();
		RelationInspectorSkin GetSkin();

		void MoveEntity(T entity, Vector2 delta);

		bool IsRoot(T entity);	// is entity an inspector target object?
		IGraphBackend<T, P> GetBackend();
	}
}
