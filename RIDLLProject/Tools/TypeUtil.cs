using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	internal static class TypeUtil
	{

		// get common interfaces and the most specific base type (not all base types)
		internal static IEnumerable<Type> GetTypesAssignableFrom( IEnumerable<Type> types )
		{
			return GetSharedInterfaces( types ).Concat( new Type[] { GetSharedBaseType( types ) } );
		}

		// return the interfaces shared between the given types
		internal static IEnumerable<Type> GetSharedInterfaces( IEnumerable<Type> types )
		{
			if ( !types.Any() )
				return Enumerable.Empty<Type>();

			var sharedInterfaces = GetInterfaces( types.First() );
			foreach ( var type in types )
			{
				var typeInterfaces = GetInterfaces( type );
				if ( !typeInterfaces.Any() )
					return Enumerable.Empty<Type>();

				sharedInterfaces = sharedInterfaces.Intersect( typeInterfaces );

				// stop if there there is no common interface
				if ( !sharedInterfaces.Any() )
					break;
			}
			return sharedInterfaces;
		}

		// return the most specific basetype shared the given types, null if there is none. no interface.
		internal static Type GetSharedBaseType( IEnumerable<Type> types )
		{
			if ( !types.Any() )
				return null;

			return types.Aggregate( ( baseT, type ) => GetCommonBaseType( baseT, type ) );
		}

		// returns the given type's interfaces or itself (if it is one)
		internal static IEnumerable<Type> GetInterfaces( Type type )
		{
			if ( type.IsInterface )
				return new[] { type };
			return type.GetInterfaces();
		}

		// returns true if the given type implements the given generic interfacetype
		internal static bool ImplementsGenericInterface( Type type, Type interfaceType )
		{
			return type.GetInterfaces().Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType ).Any();
		}

		internal static Type GetGenericInterface( Type type, Type interfaceType )
		{
			return type.GetInterfaces().Where( i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType ).FirstOrDefault();
		}

		// wrap MakeGenericType, don't throw exceptions
		internal static Type MakeGenericTypeSafe( Type type, params Type[] genericArguments )
		{
			if ( !type.IsGenericType )
				return type;

			try
			{
				return type.MakeGenericType( genericArguments );
			}
			catch ( System.ArgumentException )
			{
				return null;
			}
		}

		// returns common base class (either concrete or abstract)
		// returns null if non exists
		internal static Type GetCommonBaseType( Type typeLeft, Type typeRight )
		{
			if ( typeLeft == null || typeRight == null ) return null;

			return GetTypeHierarchy( typeLeft )
			.Intersect( GetTypeHierarchy( typeRight ) )
			.FirstOrDefault( type => !type.IsInterface );
		}

		// returns type and all its base types. no interfaces
		internal static IEnumerable<Type> GetTypeHierarchy( Type type )
		{
			if ( type == null ) yield break;
			Type typeInHierarchy = type;
			do
			{
				yield return typeInHierarchy;
				typeInHierarchy = typeInHierarchy.BaseType;
			}
			while ( typeInHierarchy != null && !typeInHierarchy.IsInterface );
		}

		internal static Type GetMostSpecificType( HashSet<Type> types )
		{
			if ( types == null || !types.Any() )
				return null;

			var nonInterfaces = types.Where( type => !type.IsInterface ).ToHashSet();

			var subTypes = new HashSet<System.Type>();
			foreach ( var type in nonInterfaces )
				TypeUtil.ReplaceSuperTypes( subTypes, type );

			if ( subTypes.Any() )
				return subTypes.First();
			return types.FirstOrDefault();
		}

		// add newEntry to types and remove all it's supertypes, if present
		internal static void ReplaceSuperTypes( HashSet<Type> types, Type newEntry )
		{
			if ( types.Contains( newEntry ) )
				return;

			foreach ( var type in GetTypeHierarchy( newEntry ) )
				if ( types.Contains( type ) )
					types.Remove( type );

			types.Add( newEntry );
			return;
		}

		// returns the loaded assembly that matches the name. 
		// returns null if there is not exactly one
		public static System.Reflection.Assembly GetAssemblyByName( string name )
		{
			return System.AppDomain.CurrentDomain.GetAssemblies().
				   SingleOrDefault( assembly => assembly.GetName().Name == name );
		}

		// valid entity types are either assignable from all the objects, or are component types that all objects share
		internal static IEnumerable<Type> GetValidEntityTypes( IEnumerable<object> objects )
		{
			return GetTypesAssignableFrom( objects.Select( obj => obj.GetType() ) ).
				Union( GetSharedComponentTypes( objects ) );
		}

		// returns the types and their basetypes of all components that are shared between the given GameObjects
		// returns empty set if any of the objects is not a GameObject (or if they share no components)
		internal static IEnumerable<Type> GetSharedComponentTypes( IEnumerable<object> objects )
		{
			if ( objects == null || !objects.Any() )
				return Enumerable.Empty<Type>();

			// all items have to be gameobjects
			var asGameObjects = objects.Select( o => o as GameObject );
			if ( asGameObjects.Any( go => go == null ) )
				return Enumerable.Empty<Type>();

			// return intersection of gameobject component types
			return asGameObjects
				.Select( GetComponentTypes )
				.Aggregate( ( shared, goCompTypes ) => shared.Intersect( goCompTypes ) );
		}

		// map gameObject to all types that its components can be assigned to (distinct)
		internal static IEnumerable<Type> GetComponentTypes( GameObject go )
		{
			return go
				.GetComponents<Component>() // seq of components held by the GO
				.SelectMany( c => GetTypeHierarchy( c.GetType() ) ) // seq of the types and supertypes of these components
				.Distinct();    // same seq, with duplicates removed
		}

		// returns the first component of the given gameobject that can be treated as compType
		// null if none found
		internal static object GetGameObjectComponentOfType( object obj, Type compType )
		{
			var asGameObject = obj as GameObject;
			if ( asGameObject == null )
				return null;

			return asGameObject
				.GetComponents<Component>()
				.FirstOrDefault( comp => compType.IsAssignableFrom( comp.GetType() ) );
		}

		// returns representations of the given objects that are assignable to targetType
		// so that the resulting objects can be used as entities of a backend which uses taretType as entity type
		internal static object MakeAssignable( object obj, Type targetType )
		{
			if ( targetType.IsAssignableFrom( obj.GetType() ) )
				return obj;

			var componentOfType = GetGameObjectComponentOfType( obj, targetType );
			if ( componentOfType != null )
			{
				return componentOfType;
			}

			Log.Error(
				string.Format(
					"unable to make object {0} of type {1} assignable to type {2}",
					obj,
					obj.GetType(),
					targetType )
					);
			return null;
		}
	}
}
