using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RelationsInspector
{
    class GraphPosSerialization
    {
        const string FileNamePrefix = "VertexPosStore";

        // vertex hash id
        // use unity unique id where possible
        // use GetHashCode as fallback
        static int GetVertexId<T>(T vertex) where T : class
        {
            if (vertex == null)
                throw new System.ArgumentException("vertex is null");

            bool isUnityObj = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
            if (isUnityObj)
                return (vertex as UnityEngine.Object).GetInstanceID();

            return vertex.GetHashCode();
        }

        // graph hash id, built from root nodes
        static int GetGraphId<T, P>(Graph<T, P> graph) where T : class
        {
            var roots = graph.Vertices.Where(v => graph.IsRoot(v));

            // we don't care about overflow
            unchecked
            {
                return roots.Aggregate( 17, (hash, root) => hash * 23 + GetVertexId(root) );
            }
        }

        // view hash id
        static int GetViewId<T, P>(Graph<T, P> graph, Type backendType) where T : class
        {
            int graphId = GetGraphId(graph);
            int backendId = backendType.ToString().GetHashCode();
            int hash = 17;
            hash = hash * 23 + graphId;
            hash = hash * 23 + backendId;
            return hash;
        }

        static string GetStorageFilePath(int hashId)
        {
            string fileName = FileNamePrefix + hashId.ToString() + ".asset";
            return System.IO.Path.Combine(ProjectSettings.LayoutCachesPath, fileName);
        }

        // graph -> storage
        static VertexPositionStorage GetVertexPositionStorage<T, P>(Graph<T, P> graph) where T : class
        {
            var storage = ScriptableObject.CreateInstance<VertexPositionStorage>();
            
            storage.vertexPositions = graph.Vertices.Select(v => new VertexPosition(GetVertexId(v), graph.GetPos(v))).ToList();

            return storage;
        }

        // save graph vertex positions to file
        public static void SaveGraphLayout<T,P>(Graph<T,P> graph, Type backendType) where T : class
        {
            if (graph == null || !graph.Vertices.Any())
                return;

            var storage = GetVertexPositionStorage(graph);
            string path = GetStorageFilePath( GetViewId(graph, backendType) );
            AssetDatabase.CreateAsset(storage, path );          
        }

        // load graph vertex positions from file
        public static bool LoadGraphLayout<T, P>(Graph<T, P> graph, Type backendType) where T : class
        {
            string path = GetStorageFilePath(GetViewId(graph, backendType));
            var storage = Util.LoadAsset<VertexPositionStorage>(path);
            if (storage == null || storage.vertexPositions == null)
                return false;

            // get a dictionary: id -> pos
            var idToPosition = storage.vertexPositions.ToDictionary(pair => pair.vertexId);

            foreach(var vertex in graph.Vertices)
            {
                int id = GetVertexId(vertex);
                if( idToPosition.ContainsKey(id) )
                {
                    graph.SetPos(vertex, idToPosition[id].vertexPosition);
                }
            }

            return true;
        }
    }
}
