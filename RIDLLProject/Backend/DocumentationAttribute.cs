using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class DocumentationAttribute : Attribute
	{
		public string url;

		public DocumentationAttribute( string url )
		{
			this.url = url;
		}
	}
}

