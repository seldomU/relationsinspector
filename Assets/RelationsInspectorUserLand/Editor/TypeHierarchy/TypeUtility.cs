using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace RelationsInspector.Backend.TypeHierarchy
{
	public static class TypeUtility
	{

		public static IEnumerable<Type> GetSubtypes(Type inspectedType, IEnumerable<Assembly> assemblies)
		{
			var assemblyTypes = assemblies.SelectMany(asm => asm.GetExportedTypes());

			if (inspectedType.IsInterface)
			{
				// todo: generic interfaces
				var implementers = assemblyTypes.Where(t => inspectedType.IsAssignableFrom(t));
				foreach (var implementer in implementers)
				{
					if (implementer.BaseType == null)
						yield return implementer;

					if (!implementer.BaseType.IsAssignableFrom(inspectedType))
						yield return implementer;

					// base type implements the interface, so this type is not directly related
				}
			}
			else
			{
				var subTypes = assemblyTypes.Where(asmType => asmType.BaseType == inspectedType);
				foreach (var type in subTypes)
					yield return type;
			}
		}

		public static IEnumerable<Type> GetBaseTypeAndInterfaces(Type inspectedType)
		{
			if (inspectedType.IsInterface)
				yield break;

			var baseType = inspectedType.BaseType;
			IEnumerable<Type> interfaces = inspectedType.GetInterfaces();
			if (baseType != null)
			{
				yield return baseType;
				interfaces = interfaces.Except( baseType.GetInterfaces() );
			}

			foreach(var interfaceType in interfaces)
				yield return interfaceType;
		}

		public static Assembly GetAssemblyByName(string name)
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == name);
		}
	}
}
