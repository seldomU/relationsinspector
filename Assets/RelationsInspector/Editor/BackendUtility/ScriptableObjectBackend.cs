using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend
{
	public class ScriptableObjectBackend<T, P> : IGraphBackend<T, P> where T : ScriptableObject
	{
		protected RelationsInspectorAPI api;
		ScriptableObjectBackendToolbar<T> toolbar;

		public virtual IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			string targetAssetDir = (targets == null) ? null : BackendTools.GetAssetDirectory(targets.FirstOrDefault() as Object);
			toolbar = new ScriptableObjectBackendToolbar<T>(api, targetAssetDir);

			return BackendTools.Convert<T>(targets);
		}

		public virtual Rect DrawContent(T entity, EntityDrawContext drawContext)
		{
			return DrawUtil.DrawContent( new GUIContent(entity.name), drawContext);
		}

		public virtual Rect OnGUI()
		{
			toolbar.OnGUI();
			return BackendTools.GetMaxRect();
		}

		public virtual void CreateEntity(Vector2 position)
		{
			toolbar.ShowNamePrompt(position);
			api.Repaint();
		}

		public void DeleteEntity(T entity)
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(entity));
			AssetDatabase.SaveAssets();
			api.RemoveEntity(entity);
		}

		public virtual string GetTooltip(T entity)
		{
			return entity.name;
		}

		public virtual void CreateRelation(T source, T target, P tag) { }	// to be implemented by subclass
		public virtual void DeleteRelation(T source, T target, P tag) { }	// to be implemented by subclass

		public virtual void OnEntityContextClick(IEnumerable<T> entities)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Remove entity"), false, () => { foreach (var e in entities) DeleteEntity(e); });
			menu.AddItem( new GUIContent("Add relation"),false, () => api.InitRelation(entities.ToArray(), default(P)) );
			menu.ShowAsContext();
		}

		public virtual void OnRelationContextClick(T source, T target, P tag)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Remove relation"), false, () => DeleteRelation(source, target, tag) );
			menu.ShowAsContext();
		}


		public virtual Color GetRelationColor(P relationTagValue)
		{
			return Color.white;
		}

		public virtual IEnumerable<Tuple<T, P>> GetRelations(T entity)
		{
			yield break; // to be implement by subclass
		}

		public void OnEntitySelectionChange(T[] selection) 
		{ 
			Selection.objects = selection.ToArray(); 
		}
	}
}
