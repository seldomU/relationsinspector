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
        void OnDestroy();
        void OnEvent(Event e);

        void AddEntity(object vertex, Vector2 position);
		void RemoveEntity(object vertex);
		void CreateRelation(object[] sourceEntities, object tag);
		void AddRelation(object source, object target, object tag);
		void RemoveRelation(object source, object target, object tag);

		void SelectEntityNodes(System.Predicate<object> doSelect);
        void Relayout();
	}
}
