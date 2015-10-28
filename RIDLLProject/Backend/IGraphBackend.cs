using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace RelationsInspector
{
	public interface IGraphBackend<T, P> where T : class
	{
		// initialize the backend object
		IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api);

		// 
		IEnumerable<Tuple<T,P>> GetRelated(T entity);
        IEnumerable<Tuple<T, P>> GetRelating(T entity);
		void CreateEntity(Vector2 position);
		void CreateRelation(T source, T target, P tag);
		void OnEntitySelectionChange(T[] selection);
        void OnUnitySelectionChange();
		Rect OnGUI();

		string GetEntityTooltip(T entity);
		string GetTagTooltip(P tag);
		Rect DrawContent(T entity, EntityDrawContext drawContext);

		void OnEntityContextClick(IEnumerable<T> entities);
		void OnRelationContextClick(T source, T target, P tag);
		Color GetRelationColor(P relationTagValue);
	}

    internal interface IGraphBackend2<T, P> where T : class
    {
        // initialize the backend object
        IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api);

        // 
        IEnumerable<Edge<T, P>> GetRelated(T entity);
        IEnumerable<Edge<T, P>> GetRelating(T entity);
        void CreateEntity(Vector2 position);
        void CreateRelation(T source, T target, P tag);
        void OnEntitySelectionChange(T[] selection);
        void OnUnitySelectionChange();
        Rect OnGUI();

        string GetEntityTooltip(T entity);
        string GetTagTooltip(P tag);
        Rect DrawContent(T entity, EntityDrawContext drawContext);

        void OnEntityContextClick(IEnumerable<T> entities);
        void OnRelationContextClick(T source, T target, P tag);
        Color GetRelationColor(P relationTagValue);
        void OnEvent(Event e);
    }

}
