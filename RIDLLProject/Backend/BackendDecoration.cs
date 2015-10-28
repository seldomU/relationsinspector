using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace RelationsInspector
{
    // decorator class for IGraphBackend version 1
    internal class BackendDecoratorV1<T, P> : IGraphBackendInternal<T, P> where T : class
    {
        IGraphBackend<T, P> backend;

        public BackendDecoratorV1(IGraphBackend<T, P> backend)
        {
            this.backend = backend;
        }

        public Type GetDecoratedType()
        {
            return backend.GetType();
        }

        // initialize the backend object
        public IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
        {
            return backend.Init(targets, api);
        }
        
        public IEnumerable<Edge<T, P>> GetRelated(T entity)
        {
            return backend.GetRelated( entity ).Select( tuple => new Edge<T, P>( entity, tuple._1, tuple._2 ) );
        }

        public IEnumerable<Edge<T, P>> GetRelating(T entity)
        {
            return backend.GetRelating(entity).Select( tuple => new Edge<T, P>( tuple._1, entity, tuple._2 ) );
        }

        public void CreateEntity(Vector2 position)
        {
            backend.CreateEntity(position);
        }

        public void CreateRelation(T source, T target, P tag)
        {
            backend.CreateRelation(source, target, tag);
        }

        public void OnEntitySelectionChange(T[] selection)
        {
            backend.OnEntitySelectionChange(selection);
        }

        public void OnUnitySelectionChange()
        {
            backend.OnUnitySelectionChange();
        }

        public Rect OnGUI()
        {
            return backend.OnGUI();
        }

        public string GetEntityTooltip(T entity)
        {
            return backend.GetEntityTooltip(entity);
        }

        public string GetTagTooltip(P tag)
        {
            return backend.GetTagTooltip(tag);
        }

        public Rect DrawContent(T entity, EntityDrawContext drawContext)
        {
            return backend.DrawContent(entity, drawContext);
        }

        public void OnEntityContextClick(IEnumerable<T> entities)
        {
            backend.OnEntityContextClick(entities);
        }

        public void OnRelationContextClick(T source, T target, P tag)
        {
            backend.OnRelationContextClick(source, target, tag);
        }

        public Color GetRelationColor(P relationTagValue)
        {
            return backend.GetRelationColor(relationTagValue);
        }

        // V1 has no OnEvent handler
        public void OnEvent(Event e){ }
    }

    // decorator class for IGraphBackend version 2
    internal class BackendDecoratorV2<T, P> : IGraphBackendInternal<T, P> where T : class
    {
        IGraphBackend2<T, P> backend;

        public BackendDecoratorV2(IGraphBackend2<T, P> backend)
        {
            this.backend = backend;
        }

        public Type GetDecoratedType()
        {
            return backend.GetType();
        }

        public IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
        {
            return backend.Init(targets, api);
        }

        public IEnumerable<Edge<T, P>> GetRelated(T entity)
        {
            return backend.GetRelated(entity);
        }

        public IEnumerable<Edge<T, P>> GetRelating(T entity)
        {
            return backend.GetRelating(entity);
        }

        public void CreateEntity(Vector2 position)
        {
            backend.CreateEntity(position);
        }

        public void CreateRelation(T source, T target, P tag)
        {
            backend.CreateRelation(source, target, tag);
        }

        public void OnEntitySelectionChange(T[] selection)
        {
            backend.OnEntitySelectionChange(selection);
        }

        public void OnUnitySelectionChange()
        {
            backend.OnUnitySelectionChange();
        }

        public Rect OnGUI()
        {
            return backend.OnGUI();
        }

        public string GetEntityTooltip(T entity)
        {
            return backend.GetEntityTooltip(entity);
        }

        public string GetTagTooltip(P tag)
        {
            return backend.GetTagTooltip(tag);
        }

        public Rect DrawContent(T entity, EntityDrawContext drawContext)
        {
            return backend.DrawContent(entity, drawContext);
        }

        public void OnEntityContextClick(IEnumerable<T> entities)
        {
            backend.OnEntityContextClick(entities);
        }

        public void OnRelationContextClick(T source, T target, P tag)
        {
            backend.OnRelationContextClick(source, target, tag);
        }

        public Color GetRelationColor(P relationTagValue)
        {
            return backend.GetRelationColor(relationTagValue);
        }

        public void OnEvent(Event e)
        {
            backend.OnEvent(e);
        }
    }
}
