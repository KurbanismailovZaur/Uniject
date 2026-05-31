using System;
using System.Collections.Generic;
using Uniject.Bindings;
using Uniject.Reflection;

namespace Uniject
{
    public class Container
    {
        private Container _parentContainer;

        private readonly Dictionary<Type, Binding> _bindings = new();

        private readonly Queue<Type> _resolvingTypes = new();
        private readonly HashSet<Type> _resolvingTypesSet = new();

        public BindingToBuilder Bind<TContract>()
        {
            var contractType = typeof(TContract);

            if (_bindings.ContainsKey(contractType))
                throw new Exception($"Type {contractType} is already bound.");

            var binding = new Binding(this, contractType);
            _bindings[contractType] = binding;
            return new BindingToBuilder(this, binding);
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
            var binding = default(Binding);

            while (!currentContainer?._bindings.TryGetValue(contractType, out binding) ?? false)
                currentContainer = currentContainer._parentContainer;

            if (binding == null)
                throw new Exception($"No binding found for type {contractType}.");

            var instance = (T)binding.GetObject();

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