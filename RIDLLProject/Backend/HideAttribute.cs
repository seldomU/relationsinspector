using System;

namespace RelationsInspector
{
	[AttributeUsage( AttributeTargets.Class )]
	public class HideAttribute : Attribute
	{
		public HideAttribute()
		{
		}
	}
}
