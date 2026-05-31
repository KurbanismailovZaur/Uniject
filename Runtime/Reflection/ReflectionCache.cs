using System;
using System.Collections.Generic;
using System.Reflection;

namespace Uniject.Reflection
{
    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, ConstructorInjectionData> _constructors = new();

        public static ConstructorInjectionData GetConstructorInjectionData(Type concreteType)
        {
            if (_constructors.TryGetValue(concreteType, out var cached))
                return cached;

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
    }
}
