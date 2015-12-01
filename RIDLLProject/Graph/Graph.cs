using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector.Extensions;
using System;

namespace RelationsInspector
{
    public class Graph<T, P> where T : class
	{
		public Dictionary<T, VertexData<T, P>> VerticesData { get; private set; }
		public IEnumerable<T> Vertices { get { return VerticesData.Keys; } }
		public HashSet<Relation<T, P>> Edges { get; private set; }
		public int VertexCount { get { return VerticesData.Count; } }
        public bool genDownwards;
        public bool genUpwards;

		public Graph()
		{
			VerticesData = new Dictionary<T, VertexData<T, P>>();
			Edges = new HashSet<Relation<T, P>>();
		}

		public Graph(Graph<T, P> source)
		{
			this.VerticesData = new Dictionary<T, VertexData<T, P>>(source.VerticesData);
			this.Edges = new HashSet<Relation<T, P>>(source.Edges);
		}

		public void CleanNullRefs()
		{
			VerticesData.RemoveWhere(pair => Util.IsBadRef(pair.Key) );
			Edges.RemoveWhere(edge => Util.IsBadRef(edge.Source) || Util.IsBadRef(edge.Target) );
		}

		public virtual void AddVertex(T vertex)
		{
            if ( vertex == null )
            {
                Log.Error( "Vertex is null." );
                return;
            }

            if ( VerticesData.ContainsKey( vertex ) )
            {
                Log.Error( "Vertex is already part of the graph: " + vertex );
                return;
            }

			VerticesData[vertex] = new VertexData<T, P>(vertex);
		}

        public bool CanRegenerate( T source, T target)
        {
            if ( this.GetChildren( source ).Contains( target ) )
                return genDownwards;
            if ( this.GetParents( source ).Contains( target ) )
                return genUpwards;
            throw new Exception( "source and target are not related" );
        }

        public virtual void RemoveVertex(T vertex)
		{
            if ( vertex == null )
            {
                Log.Error("Vertex is null.");
                return;
            }

			var affectedEdges = Edges.Where(e => e.Source == vertex || e.Target == vertex).ToArray();
			foreach (var edge in affectedEdges)
				RemoveEdge(edge);

            if ( !VerticesData.Remove( vertex ) )
            {
                Log.Error( "Vertex is not a member: " + vertex );
                return;
            }
		}

		public virtual void AddVertex(T vertex, Vector2 position)
		{
			AddVertex(vertex);
            // only set position if vertex was added
			if ( ContainsVertex( vertex ) )
				SetPos(vertex, position);
		}

		public virtual void AddEdge(Relation<T, P> relation)
		{
            if ( relation == null || relation.Source == null || relation.Target == null )
            {
                Log.Error( "Relation or vertex is null." );
                return;
            }

            if ( !VerticesData.ContainsKey( relation.Source ) )
            {
                Log.Error( "Relation source is missing from the graph: " + relation.Source );
                return;
            }

            if ( !VerticesData.ContainsKey( relation.Target ) )
            {
                Log.Error( "Relation target is missing from the graph: " + relation.Target );
                return;
            }

            if ( !Edges.Add( relation ) )
            {
                Log.Error( "Relation is already part of the graph: " + relation );
                return;
            }

			VerticesData[relation.Source].OutEdges.Add(relation);
			VerticesData[relation.Target].InEdges.Add(relation);
		}

		public virtual void RemoveEdge(Relation<T, P> relation)
		{
            if ( relation == null )
            {
                Log.Error( "Relation is null." );
                return;
            }

            if ( !Edges.Remove( relation ) )
            {
                Log.Error( "Relation is not part of the graph: " + relation );
                return;
            }

			VerticesData[relation.Source].OutEdges.Remove(relation);
			VerticesData[relation.Target].InEdges.Remove(relation);
		}

		public bool IsRoot(T vertex, bool ignoreSelfEdges = true)
		{
            var inEdges = VerticesData[ vertex ].InEdges.Get();

            return ignoreSelfEdges ?
                !inEdges.Where(edge => edge.Source == vertex).Any(): 
                !inEdges.Any();
		}

		public bool ContainsVertex(T vertex)
		{
			return VerticesData.ContainsKey(vertex);
		}

        public bool ContainsEdge( Relation<T, P> relation )
        {
            return Edges.Contains( relation );
        }

		public Vector2 GetPos(T vertex)
		{
			VertexData<T, P> data;
			if (!VerticesData.TryGetValue(vertex, out data))
			{
				Log.Error("Graph.GetPos: unknown vertex " + vertex);
				return Vector2.zero;
			}

			return data.pos;
		}

		public void SetPos(T vertex, Vector2 pos)
		{
			VertexData<T, P> data;
			if (!VerticesData.TryGetValue(vertex, out data))
			{
				Log.Error("Graph.SetPos: unknown vertex " + vertex);
				return;
			}

			data.pos = pos;
		}
	}
}
