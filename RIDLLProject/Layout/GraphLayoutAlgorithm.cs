using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector
{
	internal class GraphLayoutAlgorithm<T,P> where T : class
	{
		Vector2 minimalDisplacement = new Vector2(Mathf.Epsilon, Mathf.Epsilon);
		const float idealDistance = 1f;	// we want the algorithm to steer for this radius of space around each vertex
		const float posInitRange = 10f;	// range of random values used to generate initial vertex x and y coordinates
		const int numIterations = 200;	// run the algorithm this many times
		const float initalMaxMove = idealDistance / 5;	// move range of the first iteration (tempature)
		const float gravityStrength = 0.7f;

		Graph<T, P> graph;
		Dictionary<T, Vector2> positions;
		Dictionary<T, Vector2> forces;
		private RNG rng;
		Vector2 graphCenter;

		
		public GraphLayoutAlgorithm(Graph<T, P> graph)
		{
			this.graph = graph;
			positions = graph.GetVertexPositions();
			forces = new Dictionary<T, Vector2>();
		}

		public IEnumerator Compute()
		{
			InitVertexPositions();

			int iterationId = 0;
			while (iterationId++ < 200)
			{
				float maxMove = initalMaxMove * ( 1 - ((float)iterationId / (float)numIterations) );
				RunIteration(maxMove);
				yield return positions;
			}
		}

		public void InitVertexPositions()
		{
			rng = new RNG(4);	// chosen by fair dice role. guaranteed to be random.

			foreach(var vertex in graph.Vertices)
				positions[vertex] = new Vector2(rng.Range(0, posInitRange), rng.Range(0, posInitRange));

			UpdateCenter();
		}

		void UpdateCenter()
		{
			if (!positions.Any())
				return;

			var posSum = positions.Values.Aggregate(Vector2.zero, (sum, pos) => sum + pos);
			graphCenter = posSum * (1 / positions.Count);
		}

		void RunIteration(float maxMove)
		{
			forces.Clear();
			//

			// collect repulsive forces between all pairs of vertices
			foreach (var vertex in graph.Vertices)
			{
				Vector2 force = Vector2.zero;
				foreach (var other in graph.Vertices)
				{
					if (vertex == other)
						continue;

					force -= GetRepulsion(vertex, other);
				}
				forces[vertex] = force;
			}

			// collect attacive forces between neighbor vertices
			foreach (var edge in graph.Edges)
			{
				Vector2 attraction = GetAttraction(edge);
				forces[edge.Source] += attraction;
				forces[edge.Target] -= attraction;
			}

			// collect gravity forces
			foreach (var vertex in graph.Vertices)
				forces[vertex] += GetGravity(vertex);

			// clamp forces to temperature
			// and apply them
			float SqrMaxMove = maxMove * maxMove;
			foreach(var vertex in forces.Keys)
			{
				var force = forces[vertex];
				if (force.SqrMagnitude() > SqrMaxMove)
					force *= (maxMove / force.magnitude);

				positions[vertex] += force;
			}

			UpdateCenter();
		}

		// get force repulsing the edge source vertex from the edge target vertex
		Vector2 GetRepulsion(T source, T target)
		{
			Vector2 displacement = positions[target] - positions[source];
			if (displacement == Vector2.zero)
				displacement = minimalDisplacement;

			float distance = displacement.magnitude;
			Vector2 direction = displacement / distance;

			float forceMagnitude = idealDistance / distance;
			return direction * forceMagnitude;
		}

		// get force attracting the edge source vertex to the edge target vertex
		Vector2 GetAttraction(Edge<T, P> edge)
		{
			Vector2 displacement = positions[edge.Target] - positions[edge.Source];
			float distance = Mathf.Max(displacement.magnitude, Mathf.Epsilon);	// avoid division by 0
			Vector2 direction = displacement / distance;

			float forceMagnitude = distance * distance / idealDistance;
			return direction * forceMagnitude;
		}

		Vector2 GetGravity(T vertex)
		{
			Vector2 displacement = graphCenter - positions[vertex];
			float distance = Mathf.Max(displacement.magnitude, Mathf.Epsilon);	// avoid division by 0
			Vector2 direction = displacement / distance;

			int degree = graph.GetNeighbors(vertex).Count();

			return direction * gravityStrength * degree;	// add distance factor?
		}
	}
}
