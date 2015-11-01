using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
    internal interface IGraphBackendInternal<T, P> where T : class
    {
        System.Type GetDecoratedType();
        // initialize the backend object
        IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api);
        IEnumerable<Relation<T, P>> GetRelations(T entity);
        void CreateEntity(Vector2 position);
        void CreateRelation(T source, T target, P tag);
        void OnEntitySelectionChange(T[] selection);
        void OnUnitySelectionChange();
        Rect OnGUI();

        string GetEntityTooltip(T entity);
        string GetTagTooltip(P tag);
        Rect DrawContent(T entity, EntityDrawContext drawContext);

        void OnEntityContextClick(IEnumerable<T> entities, GenericMenu menu);
        void OnRelationContextClick(Relation<T,P> relation, GenericMenu menu);
        Color GetRelationColor(P relationTagValue);
        void OnEvent(Event e);
    }
}
