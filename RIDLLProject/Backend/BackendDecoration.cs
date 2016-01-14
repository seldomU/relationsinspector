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

		public BackendDecoratorV1( IGraphBackend<T, P> backend )
		{
			this.backend = backend;
		}

		public Type GetDecoratedType()
		{
			return backend.GetType();
		}

		public void Awake( GetAPI getAPI )
		{
			try
			{
				backend.Awake( getAPI );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public IEnumerable<T> Init( object target )
		{
			try
			{
				return backend.Init( target );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Enumerable.Empty<T>();
			}
		}

		public void OnDestroy()
		{
			try
			{
				backend.OnDestroy();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public IEnumerable<Relation<T, P>> GetRelations( T entity )
		{
			try
			{
				return backend.GetRelations( entity ).Select( rel => rel.Copy() );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Enumerable.Empty<Relation<T, P>>();
			}
		}

		public void CreateRelation( T source, T target )
		{
			try
			{
				backend.CreateRelation( source, target );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnEntitySelectionChange( T[] selection )
		{
			try
			{
				backend.OnEntitySelectionChange( selection );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnUnitySelectionChange()
		{
			try
			{
				backend.OnUnitySelectionChange();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public Rect OnGUI()
		{
			try
			{
				return backend.OnGUI();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return GUILayoutUtility.GetRect( 0, 0, new[] { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) } );
			}
		}

		public string GetEntityTooltip( T entity )
		{
			try
			{
				return backend.GetEntityTooltip( entity );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return string.Empty;
			}
		}

		public string GetTagTooltip( P tag )
		{
			try
			{
				return backend.GetTagTooltip( tag );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return string.Empty;
			}
		}

		public Rect DrawContent( T entity, EntityDrawContext drawContext )
		{
			try
			{
				return backend.DrawContent( entity, drawContext );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Util.rectZero;
			}
		}

		public void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu )
		{
			try
			{
				backend.OnEntityContextClick( entities, menu );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu )
		{
			try
			{
				backend.OnRelationContextClick( relation, menu );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public Color GetRelationColor( P relationTagValue )
		{
			try
			{
				return backend.GetRelationColor( relationTagValue );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Color.white;
			}
		}

		// V1 has no OnEvent handler
		public void OnCommand( string command )
		{
			try
			{
				backend.OnCommand( command );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}
	}

	// decorator class for IGraphBackend version 2
	internal class BackendDecoratorV2<T, P> : IGraphBackendInternal<T, P> where T : class
	{
		IGraphBackend2<T, P> backend;

		public BackendDecoratorV2( IGraphBackend2<T, P> backend )
		{
			this.backend = backend;
		}

		public Type GetDecoratedType()
		{
			return backend.GetType();
		}

		public void Awake( GetAPI getAPI )
		{
			try
			{
				backend.Awake( getAPI );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public IEnumerable<T> Init( object target )
		{
			try
			{
				return backend.Init( target );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Enumerable.Empty<T>();
			}
		}

		public void OnDestroy()
		{
			try
			{
				backend.OnDestroy();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public IEnumerable<Relation<T, P>> GetRelations( T entity )
		{
			try
			{
				return backend.GetRelations( entity ).Select( rel => rel.Copy() );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Enumerable.Empty<Relation<T, P>>();
			}
		}

		public void CreateRelation( T source, T target )
		{
			try
			{
				backend.CreateRelation( source, target );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnEntitySelectionChange( T[] selection )
		{
			try
			{
				backend.OnEntitySelectionChange( selection );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnUnitySelectionChange()
		{
			try
			{
				backend.OnUnitySelectionChange();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public Rect OnGUI()
		{
			try
			{
				return backend.OnGUI();
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return GUILayoutUtility.GetRect( 0, 0, new[] { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) } );
			}
		}

		public string GetEntityTooltip( T entity )
		{
			try
			{
				return backend.GetEntityTooltip( entity );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return string.Empty;
			}
		}

		public string GetTagTooltip( P tag )
		{
			try
			{
				return backend.GetTagTooltip( tag );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return string.Empty;
			}
		}

		public Rect DrawContent( T entity, EntityDrawContext drawContext )
		{
			try
			{
				return backend.DrawContent( entity, drawContext );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Util.rectZero;
			}
		}

		public void OnEntityContextClick( IEnumerable<T> entities, GenericMenu menu )
		{
			try
			{
				backend.OnEntityContextClick( entities, menu );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public void OnRelationContextClick( Relation<T, P> relation, GenericMenu menu )
		{
			try
			{
				backend.OnRelationContextClick( relation, menu );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}

		public Color GetRelationColor( P relationTagValue )
		{
			try
			{
				return backend.GetRelationColor( relationTagValue );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
				return Color.white;
			}
		}

		// V1 has no OnEvent handler
		public void OnCommand( string command )
		{
			try
			{
				backend.OnCommand( command );
			}
			catch ( Exception e )
			{
				Debug.LogException( e );
			}
		}
	}
}
