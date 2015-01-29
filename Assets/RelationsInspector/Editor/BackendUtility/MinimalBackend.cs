using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend
{
	public abstract class MinimalBackend<T,P> : IGraphBackend<T,P> where T : class
	{
		protected internal RelationsInspectorAPI api;

		public virtual IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{ 
			this.api = api;
			return BackendTools.Convert<T>(targets);
		}

		public virtual IEnumerable<Tuple<T, P>> GetRelations(T entity)
		{ 
			return PairWithDefaultTag( GetRelatedEntities(entity) );
		}

		public virtual void CreateEntity(Vector2 position) { }	// do nothing
		
		public virtual void DeleteEntity(T entity) { }	// do nothing
		
		public virtual void CreateRelation(T source, T target, P tag) { } // do nothing

		public virtual void DeleteRelation(T source, T target, P tag) { } // do nothing
		
		public virtual void OnEntitySelectionChange(T[] selection) 
		{ 
			if( !typeof(Object).IsAssignableFrom(typeof(T)) )
				return;

			Selection.objects = selection.Select(obj => obj as Object).ToArray();
		}

		public virtual Rect OnGUI()
		{
			return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
		}

		public virtual Rect DrawContent(T entity, EntityDrawContext drawContext)
		{
			return DrawUtil.DrawContent( GetContent(entity), drawContext);
		}

		public virtual string GetTooltip(T entity)
		{
			return GetContent(entity).tooltip;
		}

		// no entity widget context menu
		public void OnEntityContextClick(IEnumerable<T> entities){ }

		// no tag widget context menu
		public void OnRelationContextClick(T source, T target, P tag) { }

		// map relation tag value to color
		public virtual Color GetRelationColor(P relationTagValue)
		{
			return Color.white;
		}

		public virtual GUIContent GetContent(T entity)
		{
			GUIContent content;

			var asObject = entity as Object;
			if (asObject != null)
				content = EditorGUIUtility.ObjectContent(asObject, asObject.GetType());
			else 
				content =  new GUIContent(entity.ToString());

			content.tooltip = content.text;
			return content;
		}

		// convenience method. not part of the interface
		public virtual IEnumerable<T> GetRelatedEntities(T entity)
		{
			return Enumerable.Empty<T>();
		}

		// utility. turns the collection into T,P tuples, using the tag type's default value
		static IEnumerable<Tuple<T, P>> PairWithDefaultTag(IEnumerable<T> collection)
		{
			return collection.Select(item => new Tuple<T, P>(item, default(P)));
		}

	}
}
