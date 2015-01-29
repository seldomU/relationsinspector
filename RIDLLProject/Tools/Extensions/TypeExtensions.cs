using UnityEngine;
using System.Collections;
using System;

namespace RelationsInspector.Extensions
{
	public static class TypeExtensions
	{
		public static bool Implements(this Type candidateType, Type interfaceType)
		{
			return (
				interfaceType.IsInterface && 
				!candidateType.IsInterface &&
				!candidateType.IsAbstract &&
				interfaceType.IsAssignableFrom(candidateType)
				);
		}
		
		public static bool TakesCtorParam(this Type candidateType, Type paramType)
		{
			// needs to have a ctor whose single param's type can be assigned to from paramType
			foreach (var ctor in candidateType.GetConstructors())
			{
				var parameters = ctor.GetParameters();
				if (parameters.Length != 1)
					continue;

				if (!parameters[0].ParameterType.IsAssignableFrom(paramType))
					continue;

				return true;
			}
			return false;
		}
	}
}