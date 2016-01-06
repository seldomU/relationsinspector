using UnityEngine;

namespace RelationsInspector
{
	internal interface IViewParent<T, P> where T : class
	{
		void RepaintView();
		Rect GetViewRect();

		void MoveEntity( T entity, Vector2 delta );

		bool IsSeed( T entity );  // is entity derived from a inspector target object?
		IGraphBackendInternal<T, P> GetBackend();
		RelationsInspectorAPI GetAPI();
	}
}
