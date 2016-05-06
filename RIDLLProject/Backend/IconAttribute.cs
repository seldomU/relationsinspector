using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class IconAttribute : Attribute
	{
		public string iconPath;

		public IconAttribute( string iconPath )
		{
			this.iconPath = iconPath;
		}
	}
}
