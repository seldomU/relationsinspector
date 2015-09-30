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

        // returns all types implementing IGraphBackend in the eligible assemblies
        internal static List<Type> GetNonGenericBackendTypes()
        {
            return backendSearchAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where( t => IsBackendType(t) && !GetGenericArguments(t).Any(arg => arg.IsGenericParameter ) )
                .ToList();
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

        // returns the type parameters of IGraphBackend that backend uses
        // assumes that backend implements IGraphBackend
        internal static Type[] GetGenericArguments(Type backendType)
        {
            var backendInterface = GetBackendInterface(backendType);
            if (backendInterface == null)
                throw new ArgumentException(backendType + " does not implement a backend interface");

            return backendInterface.GetGenericArguments();
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

        internal static object CreateBackendDecorator(System.Type backendType)
        {
            Type decoratorType = GetDecoratorInterface(backendType).MakeGenericType( GetGenericArguments(backendType) );
            var ctorArgs = new object[] { System.Activator.CreateInstance(backendType) };
            return System.Activator.CreateInstance(decoratorType, ctorArgs );
        }


        internal static Type GetMostSpecificBackend(IList<Type> backends)
        {
            if (backends == null || backends.Count() == 0)
                return null;

            var groups = backends.GroupBy(backend => GetGenericArguments(backend).First());
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
            var attributes = backendType.GetCustomAttributes<RelationsInspectorAttribute>(true);
            return attributes.Any(attr => types.Any(t => attr.type.IsAssignableFrom(t)));
        }
    }
}
