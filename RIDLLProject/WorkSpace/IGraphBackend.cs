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
		IEnumerable<Tuple<T,P>> GetRelations(T entity);
		void CreateEntity(Vector2 position);
		void CreateRelation(T source, T target, P tag);
		void OnEntitySelectionChange(T[] selection);
		Rect OnGUI();

		string GetTooltip(T entity);
		string GetTooltip(P tag);
		Rect DrawContent(T entity, EntityDrawContext drawContext);

		void OnEntityContextClick(IEnumerable<T> entities);
		void OnRelationContextClick(T source, T target, P tag);
		Color GetRelationColor(P relationTagValue);
	}

	public class EntityMenuItem
	{
		public GUIContent content;
		public GenericMenu.MenuFunction action;
	}
}
