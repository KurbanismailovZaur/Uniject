using System;
using System.Collections.Generic;
using Uniject.Reflection;

namespace Uniject
{
    public class Container
    {
        private Container _parentContainer;

        private readonly Dictionary<Type, InstanceGetter> _bindings = new();

        private readonly Queue<Type> _resolvingTypes = new();
        private readonly HashSet<Type> _resolvingTypesSet = new();

        public void Bind<TContract>()
        {
            var contractType = typeof(TContract);
            _bindings[contractType] = new InstanceGetter(this, contractType);
        }

        public T Resolve<T>() => Resolve<T>(typeof(T));

        public T Resolve<T>(Type contractType)
        {
            if (_resolvingTypesSet.Contains(contractType))
                throw new Exception($"Circular dependency detected while resolving type {contractType}. " +
                    $"Dependencies stack: {string.Join(" → ", _resolvingTypes)} → {contractType}.");

            _resolvingTypes.Enqueue(contractType);
            _resolvingTypesSet.Add(contractType);

            var currentContainer = this;
            var instanceGetter = default(InstanceGetter);

            while (!currentContainer?._bindings.TryGetValue(contractType, out instanceGetter) ?? false)
                currentContainer = currentContainer._parentContainer;

            if (instanceGetter == null)
                throw new Exception($"No binding found for type {contractType}.");

            var instance = (T)instanceGetter.GetObject();
            
            _resolvingTypes.Dequeue();
            _resolvingTypesSet.Remove(contractType);

            return instance;
        }

        public T Instantiate<T>() => Instantiate<T>(typeof(T));

        public T Instantiate<T>(Type concreteType)
        {
            var constructorInjectionData = ReflectionCache.GetConstructorInjectionData(concreteType);
            var parametersInstances = new object[constructorInjectionData.parametersInfo.Length];

            foreach (var parameter in constructorInjectionData.parametersInfo)
            {
                var parameterInstance = Resolve<object>(parameter.ParameterType);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            return (T)constructorInjectionData.constructorInfo.Invoke(parametersInstances);
        }
    }
}