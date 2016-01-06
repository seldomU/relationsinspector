using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	public class VertexData<T, P> where T : class
	{
		T subject;
		public Vector2 pos;
		public bool unexplored;
		public EdgeSet<T, P> InEdges { get; private set; }
		public EdgeSet<T, P> OutEdges { get; private set; }

		public VertexData( T subject )
		{
			this.subject = subject;
			pos = Vector2.zero;
			InEdges = new EdgeSet<T, P>( subject, false );
			OutEdges = new EdgeSet<T, P>( subject, true );
		}

		public IEnumerable<T> GetCorrespondents()
		{
			return InEdges.GetCorrespondents().Union( OutEdges.GetCorrespondents() );
		}
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
		private Dictionary<T, HashSet<Relation<T, P>>> byCorespondent = new Dictionary<T, HashSet<Relation<T, P>>>();

		// ctor
		public EdgeSet( T subject, bool isSource )
		{
			this.subject = subject;
			this.isSource = isSource;
		}

		// returns all edges between subject and corresponent
		// returns ALL edges if correspondent is null
		public IEnumerable<Relation<T, P>> Get( T correspondent = null )
		{
			if ( correspondent == null )
				return byCorespondent.Values.SelectMany( x => x );

			if ( !byCorespondent.ContainsKey( correspondent ) )
				return Enumerable.Empty<Relation<T, P>>();

			return byCorespondent[ correspondent ];
		}

		// returns all edge sets
		public IEnumerable<HashSet<Relation<T, P>>> GetSets()
		{
			return byCorespondent.Values;
		}

		// returns all vertices this edgeset knows
		public IEnumerable<T> GetCorrespondents()
		{
			return byCorespondent.Where( pair => pair.Value.Any() ).Select( pair => pair.Key );
		}

		// add edge to the set
		public void Add( Relation<T, P> relation )
		{
			if ( relation == null )
				throw new System.ArgumentNullException();

			// make sure the subject is part of the edge
			// and in the expected role
			if ( isSource && relation.Source != subject || !isSource && relation.Target != subject )
				throw new System.ArgumentException( "Subject does not fill the expect role in edge." );

			var correspondent = isSource ? relation.Target : relation.Source;

			// ensure that the set exists
			if ( !byCorespondent.ContainsKey( correspondent ) )
				byCorespondent[ correspondent ] = new HashSet<Relation<T, P>>();

			// add the edge
			byCorespondent[ correspondent ].Add( relation );
		}

		// remove edge from the set
		public void Remove( Relation<T, P> edge )
		{
			if ( edge == null )
				throw new System.ArgumentNullException();

			var correspondent = isSource ? edge.Target : edge.Source;

			if ( !byCorespondent.ContainsKey( correspondent ) )
				throw new System.ArgumentException( "Edge correspondent unknown." );

			if ( !byCorespondent[ correspondent ].Remove( edge ) )
				throw new System.ArgumentException( "Edge not found." );
		}
	}
}
