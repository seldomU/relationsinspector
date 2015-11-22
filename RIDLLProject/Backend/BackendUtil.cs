using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
    internal static class BackendUtil
    {
        // the assemblies in which we search for backends
        static readonly Assembly[] backendSearchAssemblies = new[] 
        {
            TypeUtil.GetAssemblyByName("Assembly-CSharp-Editor"),
            typeof(RelationsInspectorWindow).Assembly
        };

        internal static readonly Dictionary<Type, Type> backendToDecorator = new Dictionary<Type, Type>
        {
            { typeof(IGraphBackend<,>), typeof(BackendDecoratorV1<,>) },
            { typeof(IGraphBackend2<,>), typeof(BackendDecoratorV2<,>) }
        };

        internal static readonly HashSet<Type> backEndInterfaces = backendToDecorator.Keys.ToHashSet();

        static readonly Type[] backendTypes = backendSearchAssemblies
            .SelectMany( asm => asm.GetTypes() )
            .Where( t => IsBackendType( t ) )
            .ToArray();

        static readonly Type autoBackendType = backendTypes
            .Where( IsAutoBackend )
            .SingleOrDefault();

        // returns all types implementing IGraphBackend in the eligible assemblies
        internal static IEnumerable<Type> GetClosedBackendTypes()
        {
            return backendTypes.Where( t => !IsOpenBackendType( t ) );
        }

        internal static Type GetBackendInterface(Type potentialBackendType)
        {
            return potentialBackendType
                .GetInterfaces()
                .Where(i => i.IsGenericType && backEndInterfaces.Contains(i.GetGenericTypeDefinition()))
                .SingleOrDefault();
        }

        // returns true if candidateType implements one of the backend interfaces
        // (the interface might have generic arguments)
        internal static bool IsBackendType(Type candidateType)
        {
            return GetBackendInterface(candidateType) != null;
        }

        internal static bool IsOpenBackendType( Type backendType )
        {
            return GetGenericArguments( backendType )
                .Any( arg => arg.IsGenericParameter );
        }

        // returns the type parameters of IGraphBackend that backend uses
        // assumes that backend implements IGraphBackend
        internal static Type[] GetGenericArguments(Type backendType)
        {
            var backendInterface = GetBackendInterface(backendType);
            if (backendInterface == null)
                throw new ArgumentException(backendType + " does not implement a backend interface");

            return backendInterface.GetGenericArguments();
        }

        internal static IEnumerable<Type> GetAutoBackendTypes( IEnumerable<Type> entityTypes )
        {
            if ( autoBackendType == null )
                return Enumerable.Empty<Type>();
            
            return entityTypes
                .Where( type => type.GetCustomAttributes<AutoBackendAttribute>( false ).Any() )
                .Select( type => autoBackendType.MakeGenericType( new[] { type } ) ); 
        }

        internal static bool IsAutoBackend( Type backendType )
        {
            // not so good
            return backendType.Name.StartsWith( ProjectSettings.AutoBackendClassName );
        }

        internal static bool? DoesBackendForceLayoutSaving(Type backendType)
        {
            var attributes = backendType.GetCustomAttributes<SaveLayoutAttribute>(true);
            if (!attributes.Any())
                return null;
            return attributes.First().doSave;
        }

        internal static Type GetDecoratorInterface(Type backendType)
        {
            Type bInterface = GetBackendInterface(backendType);
            if (bInterface == null)
                throw new ArgumentException("Expected a backend type:" + backendType);

            Type decoratorInterface;
            if (!backendToDecorator.TryGetValue(bInterface.GetGenericTypeDefinition(), out decoratorInterface))
                throw new ArgumentException("No decorator found for backend interface " + bInterface);

            return decoratorInterface;
        }

        internal static object CreateBackendDecorator(Type backendType)
        {
            Type decoratorType = GetDecoratorInterface(backendType).MakeGenericType( GetGenericArguments(backendType) );
            var ctorArgs = new object[] { Activator.CreateInstance(backendType) };
            return Activator.CreateInstance(decoratorType, ctorArgs );
        }


        internal static Type GetMostSpecificBackendType(IList<Type> backendTypes)
        {
            if (backendTypes == null || backendTypes.Count() == 0)
                return null;

            // prefer auto-backends
            var autoBackendTypes = backendTypes.Where( IsAutoBackend );
            if ( autoBackendTypes.Any() )
                return autoBackendTypes.First();

            var groups = backendTypes.GroupBy(backend => GetGenericArguments(backend).First());
            var entityTypes = groups.Select(group => group.Key).ToHashSet();

            var bestEntityType = TypeUtil.GetMostSpecificType(entityTypes);
            var bestEntityTypeGroup = groups.Single(group => group.Key == bestEntityType);
            return bestEntityTypeGroup.First();
        }

        internal static bool IsEntityTypeAssignableFromAny(Type backendType, IEnumerable<Type> types)
        {
            var entityType = GetGenericArguments(backendType).First();
            return entityType != null && types.Where(t => entityType.IsAssignableFrom(t)).Any();
        }

        // true if any of the given types can be passed as a RI attribute type
        internal static bool BackendAttributeFitsAny(Type backendType, IEnumerable<Type> types)
        {
            var attributes = backendType.GetCustomAttributes<AcceptTargetsAttribute>(true);
            return attributes.Any(attr => types.Any(t => attr.type.IsAssignableFrom(t)));
        }

        internal static Type BackendAttrType( Type backendType )
        {
            var attr = backendType.GetCustomAttributes<AcceptTargetsAttribute>( true ).FirstOrDefault();
            return (attr== null ) ? null : attr.type;
        }
    }
}
