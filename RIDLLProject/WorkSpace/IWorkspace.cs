using System.Collections.Generic;
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
		void OnEvent( Event e );

		IEnumerable<object> GetEntities();
		IEnumerable<object> GetRelations();

		void AddTargets( object[] targetsToAdd, Vector2 pos );
		void AddEntity( object vertex, Vector2 position );
		void RemoveEntity( object vertex );
		void ExpandEntity( object entity );
		void FoldEntity( object entity );

		void CreateRelation( object[] sourceEntities );
		void AddRelation( object source, object target, object tag );
		void RemoveRelation( object source, object target, object tag );
		object[] FindRelations( object entity );

		void SelectEntityNodes( System.Predicate<object> doSelect );
		void Relayout();
	}
}
