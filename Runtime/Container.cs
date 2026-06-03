using System;
using System.Collections.Generic;
using System.Reflection;
using Uniject.Attributes;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject
{
    public class Container
    {
        private Container _parentContainer;

        private readonly Dictionary<Type, Binding> _bindings = new();

        private readonly Queue<Type> _resolvingTypes = new();
        private readonly HashSet<Type> _resolvingTypesSet = new();

        public BindingToBuilder<TContract> Bind<TContract>()
        {
            var contractType = typeof(TContract);

            if (_bindings.ContainsKey(contractType))
                throw new Exception($"Type {contractType} is already bound.");

            var binding = new Binding(this, contractType);
            _bindings[contractType] = binding;
            return new BindingToBuilder<TContract>(this, binding);
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

        public void Inject(object instance)
        {
            var methodInjectionData = ReflectionCache.GetMethodInjectionData(instance.GetType());

            if (!methodInjectionData.hasInjectMethod)
                return;

            var parametersInstances = new object[methodInjectionData.parametersInfo.Length];

            foreach (var parameter in methodInjectionData.parametersInfo)
            {
                var parameterInstance = Resolve<object>(parameter.ParameterType);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            methodInjectionData.methodInfo.Invoke(instance, parametersInstances);
        }

        public void Inject(IEnumerable<object> instances)
        {
            foreach (var instance in instances)
                Inject(instance);
        }

        public T Instantiate<T>() => (T)Instantiate(typeof(T));

        public object Instantiate(Type concreteType)
        {
            var constructorInjectionData = ReflectionCache.GetConstructorInjectionData(concreteType);
            var parametersInstances = new object[constructorInjectionData.parametersInfo.Length];

            foreach (var parameter in constructorInjectionData.parametersInfo)
            {
                var parameterInstance = Resolve<object>(parameter.ParameterType);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            return constructorInjectionData.constructorInfo.Invoke(parametersInstances);
        }

        public GameObject Instantiate(GameObject prefab) => Instantiate(prefab.transform).gameObject;

        public T Instantiate<T>(T prefab) where T : Component => (T)Instantiate(prefab as Component);

        internal Component Instantiate(Component prefab)
        {
            var cloned = UnityEngine.Object.Instantiate(prefab);
            
            if (cloned.TryGetComponent<InjectionTargets>(out var injectionTargets))
                Inject(injectionTargets.Targets);
            
            return cloned;
        }
    }
}
