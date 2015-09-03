using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	public class VertexData<T, P> where T : class
	{
		T subject;
		public Vector2 pos;
		public EdgeSet<T, P> InEdges { get; private set; }
		public EdgeSet<T, P> OutEdges { get; private set; }

		public VertexData( T subject )
		{
			this.subject = subject;
			pos = Vector2.zero;
			InEdges = new EdgeSet<T, P>(subject, false);
			OutEdges = new EdgeSet<T, P>(subject, true);
		}

		public IEnumerable<T> GetCorrespondents()
		{
			return InEdges.GetCorrespondents().Union( OutEdges.GetCorrespondents() );
		}

		/*
		public IEnumerable<Edge<T,P>> InEdges( T correspondent = null)
		{
			return inEdges.Get(correspondent);
		}

		public IEnumerable<Edge<T,P>> OutEdges( T correspondent = null)
		{
			return outEdges.Get(correspondent);
		}*/
	}

    // incomming or outgoing edges of one vertex
	public class EdgeSet<T, P> where T : class
	{
        // the vertex this edgeset belongs to
        private T subject;

        // if true, byCorespondent contains outgoing edges. if false, they are incomming edges
        private bool isSource;  

		// per corresponent vertex, the set of edges between subject and the vertex. 
        // depending on isSource they are either outgoing or incomming, from subject's PoV
		private Dictionary<T, HashSet<Edge<T,P>>> byCorespondent = new Dictionary<T, HashSet<Edge<T,P>>>();

        // ctor
		public EdgeSet(T subject, bool isSource)
		{
			this.subject = subject;
			this.isSource = isSource;
		}

        // returns all edges between subject and corresponent
        // returns ALL edges if correspondent is null
		public IEnumerable<Edge<T,P>> Get( T correspondent = null)
		{
			if( correspondent == null)
				return byCorespondent.Values.SelectMany(x => x);
			
			if( !byCorespondent.ContainsKey(correspondent) )
				return Enumerable.Empty<Edge<T,P>>();

			return byCorespondent[correspondent];
		}

        // returns all edge sets
		public IEnumerable<HashSet<Edge<T, P>>> GetSets()
		{
			return byCorespondent.Values;
		}

        // returns all vertices this edgeset knows
		public IEnumerable<T> GetCorrespondents()
		{
			return byCorespondent.Where(pair => pair.Value.Any()).Select(pair => pair.Key);
		}

        // add edge to the set
		public void Add(Edge<T, P> edge)
		{
			if (edge == null)
				throw new System.ArgumentNullException();

			// make sure the subject is part of the edge
			// and in the expected role
			if (isSource && edge.Source != subject || !isSource && edge.Target != subject)
				throw new System.ArgumentException("Subject does not fill the expect role in edge.");
			
			var correspondent = isSource ? edge.Target : edge.Source;

			// ensure that the set exists
			if (!byCorespondent.ContainsKey(correspondent))
				byCorespondent[correspondent] = new HashSet<Edge<T, P>>();

			// add the edge
			byCorespondent[correspondent].Add( edge );
		}

        // remove edge from the set
		public void Remove(Edge<T, P> edge)
		{
			if(edge == null)
				throw new System.ArgumentNullException();

			var correspondent = isSource ? edge.Target : edge.Source;

			if (!byCorespondent.ContainsKey(correspondent))
				throw new System.ArgumentException("Edge correspondent unknown.");

			if( !byCorespondent[correspondent].Remove(edge) )
				throw new System.ArgumentException("Edge not found.");
		}
	}
}
