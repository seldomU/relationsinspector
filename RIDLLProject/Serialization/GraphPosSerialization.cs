using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace RelationsInspector
{
	class GraphPosSerialization
	{

		// vertex hash id
		// use unity unique id where possible
		// use GetHashCode as fallback
		static int GetVertexId<T>( T vertex ) where T : class
		{
			if ( vertex == null )
				throw new System.ArgumentException( "vertex is null" );

			var asUnityObj = vertex as UnityEngine.Object;
			return ( asUnityObj != null ) ? asUnityObj.GetInstanceID() : vertex.GetHashCode();
		}

		// view hash id
		static int GetViewId<T>( IEnumerable<T> seeds, Type backendType ) where T : class
		{
			int backendId = backendType.ToString().GetHashCode();
			int hash = 17;
			hash = hash * 23 + backendId;
			// we don't care about overflow
			unchecked
			{
				foreach ( var seed in seeds )
					hash = hash * 23 + GetVertexId( seed );
			}
			return hash;
		}

		private static string GetStorageFilePath( int hashId, Type backendType )
		{
			string backendTypeName = backendType.Name.Split( '`' )[ 0 ];
			string fileName = backendTypeName + hashId.ToString() + ".asset";
			return System.IO.Path.Combine( ProjectSettings.LayoutCachesPath, fileName );
		}

		// graph -> storage
		private static VertexPositionStorage GetVertexPositionStorage<T, P>( Graph<T, P> graph ) where T : class
		{
			var storage = ScriptableObject.CreateInstance<VertexPositionStorage>();

			storage.vertexPositions = graph.Vertices.Select( v => new VertexPosition( GetVertexId( v ), graph.GetPos( v ) ) ).ToList();
			// todo: hook these up to view
			storage.transform = new Transform2d();
			storage.widgetType = EntityWidgetType.Rect;

			return storage;
		}

		// return true if a graph of the give backend type should be saved
		private static bool ShouldGraphOfTypeBeSerialized( Type backendType )
		{
			// get first generic type parameter
			Type vertexType = BackendTypeUtil.GetEntityType( backendType );

			// fallback behaviour: return true iff it's a unity object type
			// all other types have no reliable object <-> id mapping
			bool defaultChoice = typeof( UnityEngine.Object ).IsAssignableFrom( vertexType );

			// check for LayoutSaving attribute.
			bool? userChoice = BackendTypeUtil.GetLayoutSavingChoice( backendType );

			// respect the explicit attribute choice. fall back to default if there is none
			return userChoice ?? defaultChoice;
		}

		// save graph vertex positions to file
		internal static void SaveGraphLayout<T, P>( Graph<T, P> graph, IEnumerable<T> seeds, Type backendType ) where T : class
		{
			if ( !graph.Vertices.Any() )
				return;

			// some backend types should not be included
			if ( !ShouldGraphOfTypeBeSerialized( backendType ) )
				return;

			var storage = GetVertexPositionStorage( graph );
			string path = GetStorageFilePath( GetViewId( seeds, backendType ), backendType );
			Util.ForceCreateAsset( storage, path );
		}

		// load graph vertex positions from file
		internal static bool LoadGraphLayout<T, P>( Graph<T, P> graph, IEnumerable<T> seeds, Type backendType ) where T : class
		{
			if ( !ShouldGraphOfTypeBeSerialized( backendType ) )
				return false;

			string path = GetStorageFilePath( GetViewId( seeds, backendType ), backendType );
			var storage = Util.LoadAsset<VertexPositionStorage>( path );

			if ( storage == null || storage.vertexPositions == null )
				return false;

			bool allPositionsLoaded = true;

			// get a dictionary: id -> pos
			var idToPosition = storage.vertexPositions.ToDictionary( pair => pair.vertexId );

			foreach ( var vertex in graph.Vertices )
			{
				int id = GetVertexId( vertex );

				if ( idToPosition.ContainsKey( id ) )
				{
					graph.SetPos( vertex, idToPosition[ id ].vertexPosition );
				}
				else
				{
					allPositionsLoaded = false;
				}
			}

			// todo: apply storage.widgetType storage.transform to view

			return allPositionsLoaded;
		}
	}
}
