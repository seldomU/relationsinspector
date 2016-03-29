using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;

namespace RelationsInspector
{
	public class TopologyStorage : ScriptableObject
	{
		public int[] nodes;
		public TopologyEdge[] edges;
	}

	[Serializable]
	public class TopologyEdge
	{
		public int sourceVertexId;
		public int targetVertexId;

		public TopologyEdge( int sourceId, int targetId )
		{
			sourceVertexId = sourceId;
			targetVertexId = targetId;
		}
	}

	class TopologySerializer<T, P> where T : class
	{
		public const string DefaultAssetPath = "Assets/graphTopology.asset";

		public static void DumpTopology( Graph<T, P> graph, string assetPath = DefaultAssetPath )
		{
			var storage = CreateTopologyStorage( graph );
			AssetDatabase.CreateAsset( storage, assetPath);
			AssetDatabase.SaveAssets();
		}

		public static TopologyStorage CreateTopologyStorage( Graph<T, P> graph )
		{
			var storage = ScriptableObject.CreateInstance<TopologyStorage>();
			int id=0;
			var vertexId = graph.Vertices.ToArray().ToDictionary( v => v, v => id++ );
			storage.nodes = vertexId.Values.ToArray();

			var topologyEdges = graph.Vertices.SelectMany( v => GetTopologyEdges( graph, v, x => vertexId[x] ) );
			storage.edges = topologyEdges.ToArray();
			return storage;
		}

		static IEnumerable<TopologyEdge> GetTopologyEdges( Graph<T, P> graph, T vertex, Func<T, int> vertexId )
		{
			return graph.GetOutEdges( vertex ).Select( e => new TopologyEdge( vertexId( e.Source ), vertexId( e.Target ) ) );
		}
	}
}
