using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace RelationsInspector.Backend.TypeHierarchy
{
	public static class TypeUtility
	{
		public static Assembly GetAssemblyByName( string name )
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().Single( assembly => assembly.GetName().Name == name );
		}

		public static IEnumerable<Type> GetSubtypes( Type inspectedType, IEnumerable<Assembly> assemblies )
		{
			var assemblyTypes = assemblies.SelectMany( asm => asm.GetExportedTypes() ).Where( t => t != null && t != inspectedType );

			if ( inspectedType.IsInterface )
			{
				// todo: generic interfaces
				var implementers = assemblyTypes.Where( t => inspectedType.IsAssignableFrom( t ) );
				foreach ( var implementer in implementers )
				{
					if ( implementer.BaseType == null )
						yield return implementer;
					else if ( !implementer.BaseType.IsAssignableFrom( inspectedType ) )
						yield return implementer;

					// base type implements the interface, so this type is not directly related
				}
			}
			else
			{
				var subTypes = assemblyTypes.Where( asmType => asmType.BaseType == inspectedType );
				foreach ( var type in subTypes )
					yield return type;
			}
		}
	}
}
