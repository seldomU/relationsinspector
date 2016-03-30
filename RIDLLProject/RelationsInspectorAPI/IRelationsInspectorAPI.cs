using System.Collections.Generic;
using UnityEngine;

namespace RelationsInspector
{
	public interface RelationsInspectorAPI
	{
		// draw a fresh view of the graph
		void Repaint();

		// rebuild the graph from its current target objects
		void Rebuild();

		// redo the layout on the current graph
		void Relayout();

		// enforce backend selection
		void SetBackend( System.Type backendType, bool delayed = true );

		// manipulate the graph through targets
		void ResetTargets( object[] targets, bool delayed = true );
		void ResetTargets( object[] targets, System.Type backendType, bool delayed = true );
		void AddTargets( object[] targets, bool delayed = true );

		// querying the graph
		object[] GetTargets();
		IEnumerable<object> GetEntities();
		IEnumerable<object> GetRelations();

		#region direct graph manipulation

		// manipulate the graph directly
		void AddEntity( object entity, Vector2 position, bool delayed = true );
		void RemoveEntity( object entity, bool delayed = true );
		void ExpandEntity( object entity, bool delayed = true );
		void FoldEntity( object entity, bool delayed = true );

		// add relation that has yet to be connected to its target
		void InitRelation( object[] sourceEntity, bool delayed = true );

		// get all relations involving the given entity
		object[] FindRelations( object entity );

		// add relation
		void AddRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true );

		// remove relation
		void RemoveRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true );

		#endregion

		// set node selection
		void SelectEntityNodes( System.Predicate<object> doSelect, bool delayed = true );

		// get the skin
		RelationInspectorSkin GetSkin();
	}

	internal interface RelationsInspectorAPI2
	{
		// draw a fresh view of the graph
		void Repaint();

		// rebuild the graph from its current target objects
		void Rebuild();

		// redo the layout on the current graph
		void Relayout();

		// enforce backend selection
		void SetBackend( System.Type backendType, bool delayed = true );

		// manipulate the graph through targets
		void ResetTargets( object[] targets, bool delayed = true );
		void ResetTargets( object[] targets, System.Type backendType, bool delayed = true );
		void AddTargets( object[] targets, bool delayed = true );

		// querying the graph
		object[] GetTargets();
		IEnumerable<object> GetEntities();
		IEnumerable<object> GetRelations();

		#region direct graph manipulation

		// manipulate the graph directly
		void AddEntity( object entity, Vector2 position, bool delayed = true );
		void RemoveEntity( object entity, bool delayed = true );
		void ExpandEntity( object entity, bool delayed = true );
		void FoldEntity( object entity, bool delayed = true );

		// add relation that has yet to be connected to its target
		void InitRelation( object[] sourceEntity, bool delayed = true );

		// get all relations involving the given entity
		object[] FindRelations( object entity );

		// add relation
		void AddRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true );

		// remove relation
		void RemoveRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true );

		#endregion

		// set node selection
		void SelectEntityNodes( System.Predicate<object> doSelect, bool delayed = true );

		// get the skin
		RelationInspectorSkin GetSkin();
	}
}
