using UnityEngine;
using System.Collections;

namespace RelationsInspector
{
	public class Edge<T, P> where T : class
	{
		public T Source {get; private set;}
		public T Target { get; private set; }
		public P Tag { get; private set; }

		public Edge(T source, T target, P tag)
		{
			this.Source = source;
			this.Target = target;
			this.Tag = tag;
		}

		public bool Matches(T source, T target, P tag)
		{
			return Source == source && Target == target && (Tag == null && tag == null || Tag != null && Tag.Equals(tag));
		}
	}
}
