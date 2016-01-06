namespace RelationsInspector.Extensions
{
	public static class StringExtensions
	{
		public static string RemovePrefix( this string str, string prefix )
		{
			if ( str == null )
				throw new System.ArgumentException( "str" );

			if ( prefix == null )
				throw new System.ArgumentException( "prefix" );

			if ( !str.StartsWith( prefix ) )
				throw new System.ArgumentException( "str is " + str + ". expected " + prefix );

			return str.Substring( prefix.Length );
		}
	}
}
