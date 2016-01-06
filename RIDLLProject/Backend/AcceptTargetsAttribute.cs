using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class AcceptTargetsAttribute : Attribute
	{
		public Type type;

		public AcceptTargetsAttribute( Type type )
		{
			this.type = type;
		}
	}
}
