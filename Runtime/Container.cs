using System;
using System.Collections.Generic;
using Uniject.Bindings;
using Uniject.Exceptions;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject
{
    public class Container
    {
        private Container _parentContainer;

        private readonly Dictionary<Type, Binding> _bindings = new();
        private readonly List<Type> _resolvingTypesList = new();
        private readonly HashSet<Type> _resolvingTypesSet = new();
        private readonly List<Binding> _nonLazyBindingsList = new();
        private readonly HashSet<Binding> _nonLazyBindingsSet = new();

        public BindingToBuilder<TContract> Bind<TContract>()
        {
            var contractType = typeof(TContract);

            if (_bindings.ContainsKey(contractType))
                throw new BindingException($"Type {contractType} is already bound.");

            var binding = new Binding(this, contractType);
            _bindings[contractType] = binding;
            return new BindingToBuilder<TContract>(this, binding);
        }

        public T Resolve<T>() => Resolve<T>(typeof(T));

        public T Resolve<T>(Type contractType)
        {
            EnterResolving(contractType);
            
            try
            {
                var binding = FindBinding(contractType);

                if (binding == null)
                    throw new Exception($"No binding found for type {contractType}.");

                return (T)binding.GetInstance();
            }
            finally
            {
                ExitResolving(contractType);            
            }
        }

        private void EnterResolving(Type contractType)
        {
            if (!_resolvingTypesSet.Add(contractType))
                throw new Exception($"Circular dependency detected while resolving type {contractType}. " +
                    $"Dependencies stack: {string.Join(" → ", _resolvingTypesList)} → {contractType}.");

            _resolvingTypesList.Add(contractType);
        }

        private void ExitResolving(Type contractType)
        {
            _resolvingTypesList.RemoveAt(_resolvingTypesList.Count - 1);
            _resolvingTypesSet.Remove(contractType);
        }

        private Binding FindBinding(Type contractType)
        {
            var currentContainer = this;
            var binding = default(Binding);

            while (!currentContainer?._bindings.TryGetValue(contractType, out binding) ?? false)
                currentContainer = currentContainer._parentContainer;
                
            return binding;
        }

        internal void MarkBindingNonLazy(Binding binding)
        {
            if (_nonLazyBindingsSet.Add(binding))
                _nonLazyBindingsList.Add(binding);
        }

        internal void ResolveNonLazyBindings()
        {
            foreach (var binding in _nonLazyBindingsList)
            {
                EnterResolving(binding.ContractType);
                try
                {
                    binding.PrepareNonLazyInstance();
                }
                finally
                {
                    ExitResolving(binding.ContractType);
                }
            }
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

        public GameObject Instantiate(GameObject prefab)
        {
            var cloned = UnityEngine.Object.Instantiate(prefab);
            
            if (cloned.TryGetComponent<InjectTargets>(out var injectionTargets))
                Inject(injectionTargets.Targets);
            
            return cloned;
        }


        public T Instantiate<T>(T prefab) where T : Component => (T)Instantiate(prefab as Component);

        public Component Instantiate(Component prefab)
        {
            var cloned = UnityEngine.Object.Instantiate(prefab);
            
            if (cloned.TryGetComponent<InjectTargets>(out var injectionTargets))
                Inject(injectionTargets.Targets);
            
            return cloned;
        }

        public TComponent AddComponent<TComponent>(GameObject gameObject) where TComponent : Component
        {
            return (TComponent)AddComponent(gameObject, typeof(TComponent));
        }

        public Component AddComponent(GameObject gameObject, Type componentType)
        {
            var component = gameObject.AddComponent(componentType);
            Inject(component);
            return component;
        }

        public TComponent AddComponent<TComponent>(Component component) where TComponent : Component
        {
            return (TComponent)AddComponent(component, typeof(TComponent));
        }

        public Component AddComponent(Component component, Type componentType)
        {
            component = component.gameObject.AddComponent(componentType);
            Inject(component);
            return component;
        }
    }
}