using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.SceneHierarchy
{
	public class SceneHierarchyBackend : IGraphBackend<Object, string>
	{
		public IEnumerable<Object> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
            return (targets == null) ? Enumerable.Empty<Object>() : targets.OfType<Object>();
        }
		
		// this backend is only for display
		// so don't perform any graph modification
		public void CreateEntity(Vector2 position) { }
		public void CreateRelation(Object source, Object target, string tag) { }
		public void OnEntityContextClick(IEnumerable<Object> entities) { }
		public void OnRelationContextClick(Object source, Object target, string tag) { }

		// no need for toolbar or controls
		public Rect OnGUI()
		{ 
			return BackendUtil.GetMaxRect();
		}

		// draw content
		public Rect DrawContent(Object entity, EntityDrawContext drawContext)
		{
			return DrawUtil.DrawContent(GetContent(entity), drawContext);
		}

		// 
		public Color GetRelationColor(string relationTagValue)
		{
			return Color.white;
		}

		public string GetTooltip(Object entity)
		{
			return GetContent(entity).tooltip;
		}

		public string GetTooltip(string tag)
		{
			return tag;
		}

		public void OnEntitySelectionChange(Object[] selection)
		{
			// forward our selection to unity's selection
			Selection.objects = selection.ToArray();
		}		

		public IEnumerable<Tuple<Object, string>> GetRelated(Object entity)
		{
			var asGameObject = entity as GameObject;
			if (asGameObject == null)
				yield break;

			// include sub-gameObjects
			foreach (Transform t in asGameObject.transform)
				yield return new Tuple<Object, string>(t.gameObject, string.Empty);

			// include components
			foreach (var c in asGameObject.GetComponents<Component>())
				yield return new Tuple<Object, string>(c, string.Empty);
		}

        public IEnumerable<Tuple<Object, string>> GetRelating(Object entity)
        {
            yield break;
        }

		public GUIContent GetContent(Object entity)
		{
			var content = EditorGUIUtility.ObjectContent(entity, entity.GetType());
 
			// components get their parent object's name as text
			// replace it with their type name
			if (entity is Component)
				content.text = entity.GetType().Name;

			content.tooltip = content.text;
			return content;
		}
	}
}
