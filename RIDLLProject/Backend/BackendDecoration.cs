using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;

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

        public IEnumerable<T> Init( IEnumerable<object> targets, RelationsInspectorAPI api )
        {
            return backend.Init( targets, api );
        }

        public void OnDestroy()
        {
            backend.OnDestroy();
        }

        public IEnumerable<Relation<T, P>> GetRelations( T entity )
        {
            return backend.GetRelations( entity ).Select( rel => rel.Copy() );
        }

        public void CreateEntity( Vector2 position )
        {
            backend.CreateEntity( position );
        }

        public void CreateRelation( T source, T target, P tag )
        {
            backend.CreateRelation( source, target, tag );
        }

        public void OnEntitySelectionChange( T[] selection )
        {
            backend.OnEntitySelectionChange( selection );
        }

        public void OnUnitySelectionChange()
        {
            backend.OnUnitySelectionChange();
        }

        public Rect OnGUI()
        {
            return backend.OnGUI();
        }

        public string GetEntityTooltip( T entity )
        {
            return backend.GetEntityTooltip( entity );
        }

        public string GetTagTooltip( P tag )
        {
            return backend.GetTagTooltip( tag );
        }

        public Rect DrawContent( T entity, EntityDrawContext drawContext )
        {
            return backend.DrawContent( entity, drawContext );
        }

        public void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu )
        {
            backend.OnEntityContextClick( entities, menu );
        }

        public void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu )
        {
            backend.OnRelationContextClick( relation, menu );
        }

        public Color GetRelationColor( P relationTagValue )
        {
            return backend.GetRelationColor( relationTagValue );
        }

        // V1 has no OnEvent handler
        public void OnCommand(string command){ }
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

        public void OnDestroy()
        {
            backend.OnDestroy();
        }

        public IEnumerable<Relation<T, P>> GetRelations(T entity)
        {
            return backend.GetRelations( entity ).Select( rel => rel.Copy() );
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

        public void OnEntityContextClick(IEnumerable<T> entities, GenericMenu menu)
        {
            backend.OnEntityContextClick( entities, menu );
        }

        public void OnRelationContextClick(Relation<T,P> relation, GenericMenu menu)
        {
            backend.OnRelationContextClick(relation, menu);
        }

        public Color GetRelationColor(P relationTagValue)
        {
            return backend.GetRelationColor(relationTagValue);
        }

        public void OnCommand(string command)
        {
            backend.OnCommand( command );
        }
    }
}
