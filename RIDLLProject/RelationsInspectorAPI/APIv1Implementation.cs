using System;
using System.Collections.Generic;
using UnityEngine;

namespace RelationsInspector
{
	internal class APIv1Implementation : RelationsInspectorAPI
	{
		RIInternal internalAPI;

		internal APIv1Implementation( RIInternal internalAPI )
		{
			this.internalAPI = internalAPI;
		}

		// draw a fresh view of the graph
		public void Repaint()
		{
			internalAPI.Repaint();
		}

		// rebuild the graph from the current targets
		public void Rebuild()
		{
			internalAPI.Rebuild();
		}

		// relayout the current graph
		public void Relayout()
		{
			internalAPI.Relayout();
		}

		// manipulate the graph through targets
		public void ResetTargets( object[] targets, bool delayed = true )
		{
			internalAPI.ResetTargets( targets, delayed );
		}

		public void ResetTargets( object[] targets, Type backendType, bool delayed = true )
		{
			internalAPI.ResetTargets( targets, backendType, delayed );
		}

		// if a graph exists, add targets. else create a new one from the targets
		public void AddTargets( object[] targets, bool delayed = true )
		{
			internalAPI.AddTargets( targets, Vector2.zero, delayed );
		}

		public object[] GetTargets()
		{
			return internalAPI.GetTargets();
		}

		public IEnumerable<object> GetEntities()
		{
			return internalAPI.GetEntities();
		}

		public IEnumerable<object> GetRelations()
		{
			return internalAPI.GetRelations();
		}

		// manipulate the graph directly
		public void AddEntity( object entity, Vector2 position, bool delayed = true )
		{
			internalAPI.AddEntity( entity, position, delayed );
		}

		public void RemoveEntity( object entity, bool delayed = true )
		{
			internalAPI.RemoveEntity( entity, delayed );
		}

		public void ExpandEntity( object entity, bool delayed = true )
		{
			internalAPI.ExpandEntity( entity, delayed );
		}

		public void FoldEntity( object entity, bool delayed = true )
		{
			internalAPI.FoldEntity( entity, delayed );
		}

		public void InitRelation( object[] sourceEntities, bool delayed = true )
		{
			internalAPI.InitRelation( sourceEntities, delayed );
		}

		public object[] FindRelations( object entity )
		{
			return internalAPI.FindRelations( entity );
		}

		public void AddRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true )
		{
			internalAPI.AddRelation( sourceEntity, targetEntity, tag, delayed );
		}

		public void RemoveRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true )
		{
			internalAPI.RemoveRelation( sourceEntity, targetEntity, tag, delayed );
		}

		// enforce backend selection
		public void SetBackend( Type backendType, bool delayed = true )
		{
			internalAPI.SetBackend( backendType, delayed );
		}

		public void SelectEntityNodes( Predicate<object> doSelect, bool delayed = true )
		{
			internalAPI.SelectEntityNodes( doSelect, delayed );
		}

		public RelationInspectorSkin GetSkin()
		{
			return SkinManager.GetSkin();
		}
	}
}
