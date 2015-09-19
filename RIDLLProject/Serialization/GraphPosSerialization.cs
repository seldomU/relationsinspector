using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace RelationsInspector
{
    class GraphPosSerialization
    {

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

        private static string GetStorageFilePath(int hashId, Type backendType)
        {
            string backendTypeName = backendType.Name.Split('`')[0];
            string fileName = backendTypeName + hashId.ToString() + ".asset";
            return System.IO.Path.Combine(ProjectSettings.LayoutCachesPath, fileName);
        }

        // graph -> storage
        private static VertexPositionStorage GetVertexPositionStorage<T, P>(Graph<T, P> graph) where T : class
        {
            var storage = ScriptableObject.CreateInstance<VertexPositionStorage>();
            
            storage.vertexPositions = graph.Vertices.Select(v => new VertexPosition(GetVertexId(v), graph.GetPos(v))).ToList();

            return storage;
        }

        // return true if a graph of the give backend type should be saved
        private static bool ShouldGraphOfTypeBeSaved(Type backendType)
        {
            // get first generic type parameter
            Type vertexType = BackendUtil.GetGenericArguments(backendType)[0];

            // fallback behaviour: return true iff it's a unity object type
            // all other types have no reliable object <-> id mapping
            bool defaultChoice = typeof(UnityEngine.Object).IsAssignableFrom(vertexType);

            // check for LayoutSaving attribute.
            bool? userChoice = BackendUtil.DoesBackendForceLayoutSaving(backendType);

            // respect the explicit attribute choice. fall back to default if there is none
            return userChoice ?? defaultChoice;
        }

        // save graph vertex positions to file
        internal static void SaveGraphLayout<T,P>(Graph<T,P> graph, Type backendType) where T : class
        {
            if (graph == null || !graph.Vertices.Any())
                return;

            // some backend types should not be included
            if (!ShouldGraphOfTypeBeSaved(backendType))
                return;

            bool saveByDefault = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

            var storage = GetVertexPositionStorage(graph);
            string path = GetStorageFilePath( GetViewId(graph, backendType), backendType);
            Util.ForceCreateAsset(storage, path );          
        }

        // load graph vertex positions from file
        internal static bool LoadGraphLayout<T, P>(Graph<T, P> graph, Type backendType) where T : class
        {
            string path = GetStorageFilePath(GetViewId(graph, backendType), backendType);
            var storage = Util.LoadAsset<VertexPositionStorage>(path);

            if (storage == null || storage.vertexPositions == null)
                return false;

            // get a dictionary: id -> pos
            var idToPosition = storage.vertexPositions.ToDictionary(pair => pair.vertexId);

            foreach(var vertex in graph.Vertices)
            {
                int id = GetVertexId(vertex);

                if ( idToPosition.ContainsKey(id) )
                    graph.SetPos(vertex, idToPosition[id].vertexPosition);
            }

            return true;
        }
    }
}
