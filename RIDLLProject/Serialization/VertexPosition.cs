using System;
using UnityEngine;

namespace RelationsInspector
{
	[Serializable]
	public class VertexPosition
	{
		public int vertexId;
		public Vector2 vertexPosition;

		public VertexPosition( int id, Vector2 pos )
		{
			this.vertexId = id;
			this.vertexPosition = pos;
		}
	}
}
