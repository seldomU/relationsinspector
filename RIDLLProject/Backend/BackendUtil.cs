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
            ReflectionUtil.GetAssemblyByName("Assembly-CSharp-Editor"),
            typeof(RelationsInspectorWindow).Assembly
        };

        internal static readonly Dictionary<Type, Type> backendToDecorator = new Dictionary<Type, Type>
        {
            { typeof(IGraphBackend<,>), typeof(BackendDecoratorV1<,>) },
            { typeof(IGraphBackend2<,>), typeof(BackendDecoratorV2<,>) }
        };

        internal static readonly HashSet<Type> backEndInterfaces = backendToDecorator.Keys.ToHashSet();

        // returns all types implementing IGraphBackend in the eligible assemblies
        internal static List<Type> FindBackends()
        {
            return backendSearchAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(IsBackendType)
                .ToList();
        }

        internal static Type GetBackendInterface(Type potentialBackendType)
        {
            return potentialBackendType
                .GetInterfaces()
                .Where(i => i.IsGenericType && backEndInterfaces.Contains(i.GetGenericTypeDefinition()))
                .SingleOrDefault();
        }

        // returns true if candidateType implements IGraphBackend without any generic type parameters
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
    }
}
