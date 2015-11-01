using UnityEngine;
using System.Collections.Generic;

namespace RelationsInspector
{
    internal interface IGraphView<T, P> where T : class
	{
		void Draw();
		void OnToolbarGUI();
		void HandleEvent(Event ev);

		Rect GetViewRect(Rect source);
		Vector2 GetGraphPosition(Vector2 windowPos);
		T GetEntityAtPosition(Vector2 position);

		void CreateEdge(IEnumerable<T> sourceEntities, P tag);

		void OnRemovedEntity(T entity);

		void SelectEntityNodes(System.Predicate<object> doSelect);
		void FitViewRectToGraph(bool immediately);
	}
}
