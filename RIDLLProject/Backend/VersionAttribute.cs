using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class VersionAttribute : Attribute
	{
		public string version;

		public VersionAttribute( string version )
		{
			this.version = version;
		}
	}
}
