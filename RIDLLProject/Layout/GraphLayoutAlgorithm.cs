using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	[System.Serializable]
	internal class GraphLayoutParameters
	{
		public float idealDistance = 1f; // steer for this radius of space around each vertex
		public float posInitRange = 5f; // range of random values used to generate initial vertex x and y coordinates
		public int numIterations = 200;  // run the algorithm this many times
		public float initalMaxMove = 1 / 5f;  // move range of the first iteration (tempature). relative to ideal distance
		public float gravityStrength = 3f;
	}

	internal class GraphLayoutAlgorithm<T, P> where T : class
	{
		Vector2 minimalDisplacement = new Vector2( Mathf.Epsilon, Mathf.Epsilon );

		GraphLayoutParameters settings;
		Dictionary<T, Vector2> forces;

		// graph data
		// make copys of everything. the graph might change while this is running.
		T[] vertices;
		HashSet<Relation<T, P>> edges;
		Dictionary<T, Vector2> position;
		Dictionary<T, int> degree;

		public GraphLayoutAlgorithm( Graph<T, P> graph )
		{
			vertices = graph.Vertices.ToArray();
			position = graph.Vertices.ToDictionary( v => v, v => graph.GetPos( v ) );
			degree = graph.Vertices.ToDictionary( v => v, v => graph.GetNeighbors( v ).Count() );
			edges = graph.Edges.ToHashSet();

			forces = new Dictionary<T, Vector2>();
		}

		public IEnumerator Compute( GraphLayoutParameters settings )
		{
			if ( !position.Any() )
				yield break;

			this.settings = settings;
			int iterationId = 0;
			while ( iterationId++ < settings.numIterations )
			{
				float maxMove = settings.initalMaxMove * ( 1 - ( (float) iterationId / (float) settings.numIterations ) );
				RunIteration( maxMove );
				yield return position;
			}
		}

		static Vector2 Average( IEnumerable<Vector2> vectors )
		{
			var posSum = vectors.Aggregate( Vector2.zero, ( sum, pos ) => sum + pos );
			return posSum * ( 1 / vectors.Count() );
		}

		void RunIteration( float maxMove )
		{
			forces.Clear();
			Vector2 graphCenter = Average( position.Values );

			// collect repulsive forces between all pairs of vertices
			foreach ( var vertex in vertices )
			{
				Vector2 force = Vector2.zero;
				foreach ( var other in vertices )
				{
					if ( vertex == other )
						continue;

					force -= GetRepulsion( vertex, other );
				}
				forces[ vertex ] = force;
			}

			// collect attractive forces between neighbor vertices
			foreach ( var edge in edges )
			{
				Vector2 attraction = GetAttraction( edge );
				forces[ edge.Source ] += attraction;
				forces[ edge.Target ] -= attraction;
			}

			// collect gravity forces
			foreach ( var vertex in vertices )
				forces[ vertex ] += GetGravity( vertex, graphCenter );

			// clamp forces to temperature
			// and apply them
			float SqrMaxMove = maxMove * maxMove;
			foreach ( var vertex in forces.Keys )
			{
				var force = forces[ vertex ];
				if ( force.SqrMagnitude() > SqrMaxMove )
					force *= ( maxMove / force.magnitude );

				position[ vertex ] += force;
			}
		}

		// get force repulsing the edge source vertex from the edge target vertex
		Vector2 GetRepulsion( T source, T target )
		{
			Vector2 displacement = position[ target ] - position[ source ];
			if ( displacement == Vector2.zero )
				displacement = minimalDisplacement;

			float distance = displacement.magnitude;
			Vector2 direction = displacement / distance;

			float forceMagnitude = settings.idealDistance / distance;
			return direction * forceMagnitude;
		}

		// get force attracting the edge source vertex to the edge target vertex
		Vector2 GetAttraction( Relation<T, P> edge )
		{
			Vector2 displacement = position[ edge.Target ] - position[ edge.Source ];
			float distance = Mathf.Max( displacement.magnitude, Mathf.Epsilon );    // avoid division by 0
			Vector2 direction = displacement / distance;

			float forceMagnitude = distance * distance / settings.idealDistance;
			return direction * forceMagnitude;
		}

		// get force attracting the vertex towards the graph center
		Vector2 GetGravity( T vertex, Vector2 attractor )
		{
			Vector2 displacement = attractor - position[ vertex ];
			float distance = Mathf.Max( displacement.magnitude, Mathf.Epsilon );    // avoid division by 0
			Vector2 direction = displacement / distance;

			return direction * settings.gravityStrength * distance;
		}
	}
}
