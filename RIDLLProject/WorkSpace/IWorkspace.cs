using UnityEngine;

namespace RelationsInspector
{
	internal interface IWorkspace
	{
		void OnToolbarGUI();
		Rect OnControlsGUI();
		void OnGUI( Rect drawRect );
		void Update();
		void OnSelectionChange();

		void AddEntity(object vertex, Vector2 position);
		void RemoveEntity(object vertex);
		void CreateEdge(object[] sourceEntities, object tag);
		void AddEdge(object source, object target, object tag);
		void RemoveEdge(object source, object target, object tag);

		void SelectEntityNodes(System.Predicate<object> doSelect);
	}
}
