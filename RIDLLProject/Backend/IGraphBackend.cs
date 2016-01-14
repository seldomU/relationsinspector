using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RelationsInspector
{
	public interface IGraphBackend<T, P> where T : class
	{
		void Awake( GetAPI getAPI );
		IEnumerable<T> Init( object target );
		void OnDestroy();
		IEnumerable<Relation<T, P>> GetRelations( T entity );
		void CreateRelation( T source, T target );
		void OnEntitySelectionChange( T[] selection );
		void OnUnitySelectionChange();
		Rect OnGUI();

		string GetEntityTooltip( T entity );
		string GetTagTooltip( P tag );
		Rect DrawContent( T entity, EntityDrawContext drawContext );

		void OnEntityContextClick( IEnumerable<T> entities, GenericMenu contextMenu );
		void OnRelationContextClick( Relation<T, P> relation, GenericMenu contextMenu );
		Color GetRelationColor( P relationTagValue );
		void OnCommand( string command );
	}

	internal interface IGraphBackend2<T, P> where T : class
	{
		// initialize the backend object
		void Awake( GetAPI getAPI );
		IEnumerable<T> Init( object target );
		void OnDestroy();
		IEnumerable<Relation<T, P>> GetRelations( T entity );
		void CreateRelation( T source, T target );
		void OnEntitySelectionChange( T[] selection );
		void OnUnitySelectionChange();
		Rect OnGUI();

		string GetEntityTooltip( T entity );
		string GetTagTooltip( P tag );
		Rect DrawContent( T entity, EntityDrawContext drawContext );

		void OnEntityContextClick( IEnumerable<T> entities, GenericMenu contextMenu );
		void OnRelationContextClick( Relation<T, P> relation, GenericMenu contextMenu );
		Color GetRelationColor( P relationTagValue );
		void OnCommand( string command );
	}

}
