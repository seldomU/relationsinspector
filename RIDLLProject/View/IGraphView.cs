using UnityEngine;
using System.Collections.Generic;

namespace RelationsInspector
{
	internal interface IGraphView<T, P> where T : class
	{
		void Draw();
		void OnToolbarGUI();
		void HandleEvent( Event ev );

		Vector2 GetGraphPosition( Vector2 windowPos );
		void CreateEdge( IEnumerable<T> sourceEntities );

		void OnRemovedEntity( T entity );
		void OnRemovedRelation( Relation<T, P> relation );

		void SelectEntityNodes( System.Predicate<object> doSelect );
		void FitViewRectToGraph( bool immediately );
	}
}
