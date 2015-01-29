using UnityEngine;
using System.Collections;

namespace RelationsInspector.Backend.BenchmarkTool
{
	public class Number
	{
		public int value;

		public override string ToString()
		{
			return value.ToString();
		}
	}

	public enum NumberRelation { Greater, Smaller, Unspecified };
}
