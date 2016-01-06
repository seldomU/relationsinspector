using System;
using UnityEngine;

namespace RelationsInspector
{
    internal class APIv1Implementation : RelationsInspectorAPI
    {
        RIInternal internalAPI;

        internal APIv1Implementation(RIInternal internalAPI)
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
        public void ResetTargets(object[] targets)
        {
            internalAPI.ResetTargets(targets);
        }

        // if a graph exists, add targets. else create a new one from the targets
        public void AddTargets(object[] targets)
        {
            internalAPI.AddTargets(targets, Vector2.zero);
        }

        public object[] GetTargets()
        {
            return internalAPI.GetTargets();
        }

        // manipulate the graph directly
        public void AddEntity(object entity, Vector2 position)
        {
            internalAPI.AddEntity(entity, position);
        }

        public void RemoveEntity(object entity)
        {
            internalAPI.RemoveEntity(entity);
        }

        public void ExpandEntity( object entity )
        {
            internalAPI.ExpandEntity( entity );
        }

        public void FoldEntity( object entity )
        {
            internalAPI.FoldEntity( entity );
        }

        public void InitRelation(object[] sourceEntities)
        {
            internalAPI.InitRelation(sourceEntities);
        }

        public object[] FindRelations( object entity )
        {
            return internalAPI.FindRelations( entity );
        }

        public void AddRelation(object sourceEntity, object targetEntity, object tag)
        {
            internalAPI.AddRelation(sourceEntity, targetEntity, tag);
        }

        public void RemoveRelation(object sourceEntity, object targetEntity, object tag)
        {
            internalAPI.RemoveRelation(sourceEntity, targetEntity, tag);
        }

        // enforce backend selection
        public void SetBackend(Type backendType)
        {
            internalAPI.SetBackend(backendType);
        }

        public void SelectEntityNodes(Predicate<object> doSelect)
        {
            internalAPI.SelectEntityNodes(doSelect);
        }
    }
}
