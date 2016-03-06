using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RelationsInspector.Backend.AutoBackend
{
	public class ReflectionUtil
	{
		public static IEnumerable<FieldInfo> GetAttributeFields<P, S>()
		{
			return typeof( P )
				.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
				.Where( fInfo => HasAttribute( fInfo, typeof( S ) ) );
		}

		static bool HasAttribute( MemberInfo mInfo, Type attrType )
		{
			return mInfo.GetCustomAttributes( attrType, false ).Any();
		}

		public static IEnumerable<T> GetValues<T>( FieldInfo fInfo, T fObj ) where T : class
		{
			// how to get T objects out of different field types
			var extractValues = new Dictionary<Type, Func<object, IEnumerable<T>>>()
			{
				{typeof(T), obj => new T[] { obj as T } },
				{typeof(List<T>), obj => ( obj as List<T> ) ?? Enumerable.Empty<T>() },
				{typeof(T[]), obj => ( obj as T[] ) ?? Enumerable.Empty<T>() }
			};

			if ( !extractValues.ContainsKey( fInfo.FieldType ) )
				return Enumerable.Empty<T>();

			return extractValues[ fInfo.FieldType ]
				.Invoke( fInfo.GetValue( fObj ) )
				.Where( v => v != null );
		}
	}
}
