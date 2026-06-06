using System;
using System.Collections.Generic;
using System.Reflection;
using Uniject.Attributes;
using UnityEngine;

namespace Uniject.Reflection
{
    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, ConstructorInjectionData> _constructors = new();

        private static readonly Dictionary<Type, MethodInjectionData> _methods = new();


        public static ConstructorInjectionData GetConstructorInjectionData(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (_constructors.TryGetValue(concreteType, out var cached))
                return cached;

            TypeValidator.ThrowIfCantBeInstantiatedFromConstructor(concreteType);

            var best = default(ConstructorInfo);
            var bestParameters = Array.Empty<ParameterInfo>();

            foreach (var constructor in concreteType.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                if (parameters.Length >= bestParameters.Length)
                {
                    best = constructor;
                    bestParameters = parameters;
                }
            }

            if (best == null)
                throw new Exception($"No public constructor found for type {concreteType}.");

            return _constructors[concreteType] = new ConstructorInjectionData(best, bestParameters);
        }

        public static MethodInjectionData GetMethodInjectionData(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (_methods.TryGetValue(concreteType, out var cached))
                return cached;

            foreach (var method in concreteType.GetMethods())
            {
                if (method.GetCustomAttributes(typeof(InjectAttribute), false).Length == 0)
                    continue;

                var data = new MethodInjectionData(method, method.GetParameters(), true);
                return _methods[concreteType] = data;
            }

            return _methods[concreteType] = new MethodInjectionData(null, Array.Empty<ParameterInfo>(), false);
        }
    }
}
