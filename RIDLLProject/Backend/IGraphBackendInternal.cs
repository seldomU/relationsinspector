using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	internal interface IGraphBackendInternal<T, P> where T : class
	{
		System.Type GetDecoratedType();
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

		void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu );
		void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu );
		Color GetRelationColor( P relationTagValue );
		void OnCommand( string command );
	}
}
