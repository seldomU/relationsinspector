using UnityEngine;
using System.Collections.Generic;

namespace RelationsInspector
{
	class VertexPositionStorage : ScriptableObject
	{
		public List<VertexPosition> vertexPositions;
		public Transform2d transform;
		public EntityWidgetType widgetType;
	}
}
