using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class TitleAttribute : Attribute
	{
		public string title;

		public TitleAttribute( string title )
		{
			this.title = title;
		}
	}
}
