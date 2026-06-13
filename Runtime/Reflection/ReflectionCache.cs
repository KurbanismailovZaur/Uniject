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

            if (concreteType.IsInterface)
                throw new ArgumentException($"Type {concreteType} can not be instantiated because it is an interface.", 
                    nameof(concreteType));

            if (concreteType.IsAbstract)
                throw new ArgumentException($"Type {concreteType} can not be instantiated because it is abstract.", 
                    nameof(concreteType));

            if (typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} can not be instantiated from constructor because it is a Unity Component.",
                    nameof(concreteType));

            var injected = default(ConstructorInfo);
            var best = default(ConstructorInfo);
            var bestParameters = Array.Empty<ParameterInfo>();

            foreach (var constructor in concreteType.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                var hasInjectAttribute = constructor.IsDefined(typeof(InjectAttribute), false);

                if (hasInjectAttribute)
                {
                    if (injected != null)
                        throw new InvalidOperationException($"Multiple [Inject] constructors found for type {concreteType}.");

                    injected = constructor;
                    best = constructor;
                    bestParameters = parameters;
                    continue;
                }

                if (injected == null && (best == null || parameters.Length > bestParameters.Length))
                {
                    best = constructor;
                    bestParameters = parameters;
                }
            }

            if (best == null)
                throw new InvalidOperationException($"No public constructor found for type {concreteType}.");

            return _constructors[concreteType] = new ConstructorInjectionData(best, bestParameters);
        }

        public static MethodInjectionData GetMethodInjectionData(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (_methods.TryGetValue(concreteType, out var cached))
                return cached;

            var injectMethod = default(MethodInfo);

            foreach (var method in concreteType.GetMethods())
            {
                if (!method.IsDefined(typeof(InjectAttribute), false))
                    continue;

                if (injectMethod != null)
                    throw new InvalidOperationException($"Multiple inject methods found for type {concreteType}.");

                injectMethod = method;
            }

            if (injectMethod == null)
                return _methods[concreteType] = new MethodInjectionData(null, Array.Empty<ParameterInfo>(), false);

            var parameters = injectMethod.GetParameters();

            if (parameters.Length == 0)
                throw new InvalidOperationException($"Inject method with 0 parameters found for type {concreteType}.");

            var data = new MethodInjectionData(injectMethod, injectMethod.GetParameters(), true);
            return _methods[concreteType] = data;    
        }
    }
}
