using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // manipulate the graph through targets
        public void ResetTargets(object[] targets)
        {
            internalAPI.ResetTargets(targets);
        }

        // if a graph exists, add targets. else create a new one from the targets
        public void AddTargets(object[] targets)
        {
            internalAPI.AddTargets(targets);
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

        public void InitRelation(object[] sourceEntities, object tag)
        {
            internalAPI.InitRelation(sourceEntities, tag);
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

        public void SelectEntityNodes(System.Predicate<object> doSelect)
        {
            internalAPI.SelectEntityNodes(doSelect);
        }
    }
}
