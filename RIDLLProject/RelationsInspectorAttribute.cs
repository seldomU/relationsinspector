using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationsInspector
{
	[AttributeUsage(AttributeTargets.Class)]
	public class RelationsInspectorAttribute : System.Attribute
	{
		public Type type;

		public RelationsInspectorAttribute(Type type)
		{
			this.type = type;
		}
	}
}
