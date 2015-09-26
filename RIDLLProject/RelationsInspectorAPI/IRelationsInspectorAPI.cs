using UnityEngine;
using System.Collections;

namespace RelationsInspector
{
	public interface RelationsInspectorAPI
	{
		// draw a fresh view of the graph
		void Repaint();

		// enforce backend selection
		void SetBackend(System.Type backendType);

		// manipulate the graph through targets
		void ResetTargets(object[] targets);
		void AddTargets(object[] targets);

		#region direct graph manipulation

		// manipulate the graph directly
		void AddEntity(object entity, Vector2 position);
		void RemoveEntity(object entity);

		// add relation that has yet to be connected to its target
		void InitRelation(object[] sourceEntity, object tag);

		// add relation
		void AddRelation(object sourceEntity, object targetEntity, object tag);

		// remove relation
		void RemoveRelation(object sourceEntity, object targetEntity, object tag);

		#endregion

		// set node selection
		void SelectEntityNodes(System.Predicate<object> doSelect);
	}

    internal interface RelationsInspectorAPI2
    {
        // draw a fresh view of the graph
        void Repaint();

        // enforce backend selection
        void SetBackend(System.Type backendType);

        // manipulate the graph through targets
        void ResetTargets(object[] targets);
        void AddTargets(object[] targets);

        #region direct graph manipulation

        // manipulate the graph directly
        void AddEntity(object entity, Vector2 position);
        void RemoveEntity(object entity);

        // add relation that has yet to be connected to its target
        void InitRelation(object[] sourceEntity, object tag);

        // add relation
        void AddRelation(object sourceEntity, object targetEntity, object tag);

        // remove relation
        void RemoveRelation(object sourceEntity, object targetEntity, object tag);

        #endregion

        // set node selection
        void SelectEntityNodes(System.Predicate<object> doSelect);

        void SendEvent(Event e);
    }
}
