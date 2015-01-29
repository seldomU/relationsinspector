using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	internal static class ReflectionUtil		// based on https://gist.github.com/akimboyko/4442677
	{
		
		// return type for given typeName, search all assemblies
		internal static Type GetType(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
				return null;

			// remove any trailing generic arguments
			typeName = typeName.Split('[')[0];

			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (var asm in assemblies)
			{
				var type = asm.GetType(typeName, false);
				if (type != null)
					return type;
			}
			return null;
		}
		
		// get common interfaces and the most specific base type (not all base types)
		internal static IEnumerable<Type> GetTypesAssignableFrom(IEnumerable<Type> types)
		{
			return GetSharedInterfaces(types).Concat( new Type[]{ GetSharedBaseType(types) } );
		}

		// return the interfaces shared between the given types
		internal static IEnumerable<Type> GetSharedInterfaces(IEnumerable<Type> types)
		{
			if (!types.Any())
				return Enumerable.Empty<Type>();

			var sharedInterfaces = GetInterfaces(types.First());
			foreach(var type in types)
			{
				var typeInterfaces = GetInterfaces(type);
				if (!typeInterfaces.Any())
					return Enumerable.Empty<Type>();

				sharedInterfaces = sharedInterfaces.Intersect( typeInterfaces );

				// stop if there there is no common interface
				if (!sharedInterfaces.Any())
					break;
			}
			return sharedInterfaces;
		}

		// return the most specific basetype shared the given types, null if there is none. no interface.
		internal static Type GetSharedBaseType(IEnumerable<Type> types)
		{
			if (!types.Any())
				return null;

			return types.Aggregate( (baseT, type) => GetCommonBaseType(baseT, type) );			
		}
		
		// returns the given type's interfaces or itself (if it is one)
		internal static IEnumerable<Type> GetInterfaces(Type type)
		{
			if (type.IsInterface)
				return new[] { type };
			return type.GetInterfaces();
		}

		// returns true if the given type implements the given generic interfacetype
		internal static bool ImplementsGenericInterface(Type type, Type interfaceType)
		{
			return type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).Any();
		}

		internal static Type GetGenericInterface(Type type, Type interfaceType)
		{
			return type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).FirstOrDefault();
		}

		// wrap MakeGenericType, don't throw exceptions
		internal static Type MakeGenericTypeSafe(Type type, params Type[] genericArguments)
		{
			if (!type.IsGenericType)
				return type;

			try
			{
				return type.MakeGenericType(genericArguments);
			}
			catch (System.ArgumentException)
			{
				return null;
			}
		}

		// searching for common base class (either concrete or abstract)
		internal static Type GetCommonBaseType(Type typeLeft, Type typeRight)
		{
			if (typeLeft == null || typeRight == null) return null;

			return GetTypeHierarchy(typeLeft)
			.Intersect(GetTypeHierarchy(typeRight))
			.FirstOrDefault(type => !type.IsInterface);
		}

		// basetype including the type itself. no interfaces
		internal static IEnumerable<Type> GetTypeHierarchy(Type type)
		{
			if (type == null) yield break;
			Type typeInHierarchy = type;
			do
			{
				yield return typeInHierarchy;
				typeInHierarchy = typeInHierarchy.BaseType;
			}
			while (typeInHierarchy != null && !typeInHierarchy.IsInterface);
		}

		internal static Type GetMostSpecificType(HashSet<Type> types)
		{
			if(types == null || !types.Any() )
				return null;

			var nonInterfaces = types.Where( type => !type.IsInterface ).ToHashSet();

			var subTypes = new HashSet<System.Type>();
			foreach (var type in nonInterfaces)
				ReflectionUtil.ReplaceSuperTypes(subTypes, type);

			if (subTypes.Any())
				return subTypes.First();
			return types.FirstOrDefault();
		}

		// add newEntry to types and remove all it's supertypes, if present
		internal static void ReplaceSuperTypes(HashSet<Type> types, Type newEntry)
		{
			if(types.Contains(newEntry))
				return;

			foreach (var type in GetTypeHierarchy(newEntry))
				if (types.Contains(type))
					types.Remove(type);

			types.Add(newEntry);
			return;
		}

		public static System.Reflection.Assembly GetAssemblyByName(string name)
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().
				   SingleOrDefault(assembly => assembly.GetName().Name == name);
		}
	}
}
