using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
    public static class BackendUtil
    {
        // the assemblies in which we search for backends
        static readonly Assembly[] backendSearchAssemblies = new[] 
        {
            ReflectionUtil.GetAssemblyByName("Assembly-CSharp-Editor"),
            typeof(RelationsInspectorWindow).Assembly
        };

        // returns all types implementing IGraphBackend in the eligible assemblies
        public static List<Type> FindBackends()
        {
            return backendSearchAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(IsBackendType)
                .ToList();
        }

        // returns true if candidateType implements IGraphBackend without any generic type parameters
        public static bool IsBackendType(Type candidateType)
        {
            Type backendInterface = ReflectionUtil.GetGenericInterface(candidateType, typeof(IGraphBackend<,>));
            if (backendInterface == null)
                return false;

            if (backendInterface.GetGenericArguments().Any(arg => arg.IsGenericParameter))
                return false;
            return true;
        }

        // returns the type parameters of IGraphBackend that backend uses
        // assumes that backend implements IGraphBackend
        public static Type[] GetGenericArguments(Type backendType)
        {
            var backendInterface = ReflectionUtil.GetGenericInterface(backendType, typeof(IGraphBackend<,>));
            if (backendInterface == null)
                throw new ArgumentException(backendType + " does not implement IGraphBackend");
            return backendInterface.GetGenericArguments();
        }

        public static bool? DoesBackendForceLayoutSaving(Type backendType)
        {
            var attributes = backendType.GetCustomAttributes<SaveLayoutAttribute>(true);
            if (!attributes.Any())
                return null;
            return attributes.First().doSave;
        }
    }
}
