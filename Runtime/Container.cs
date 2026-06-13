using System;
using System.Collections.Generic;
using Uniject.Bindings;
using Uniject.Collections;
using Uniject.Exceptions;
using Uniject.Lifecycle;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject
{
    public class Container : IDisposable
    {
        private Container _parentContainer;

        private readonly Dictionary<Type, Binding> _bindings = new();
        private readonly List<Type> _bindingsTypes = new();
        private readonly OrderedSet<Type> _resolvingTypes = new();
        private readonly OrderedSet<object> _injectQueue = new();

        public BindingToBuilder<TContract> Bind<TContract>() => new(this, CreateBinding(typeof(TContract)));

        public BindingToBuilder Bind(Type contractType) => new(this, CreateBinding(contractType));

        private Binding CreateBinding(Type contractType)
        {
            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));

            if (_bindings.ContainsKey(contractType))
                throw new BindingException($"Type {contractType} is already bound.");

            var binding = new Binding(this, contractType);
            _bindings[contractType] = binding;
            _bindingsTypes.Add(contractType);
            return binding;
        }

        public void BindInstance<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Bind<T>().FromInstance(instance);
        }

        public void BindInstances(params object[] instances)
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));

            foreach (var instance in instances)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instances));

                Bind(instance.GetType()).FromInstance(instance);
            }
        }

        public T Resolve<T>() => (T)Resolve(typeof(T));
        
        public object Resolve(Type contractType)
        {
            EnterResolving(contractType);
            
            try
            {
                var binding = FindBinding(contractType);

                if (binding == null)
                    throw new Exception($"No binding found for type {contractType}. " + 
                        $"Dependencies stack: {string.Join(" ← ", _resolvingTypes)}.");

                return binding.GetInstance();
            }
            finally
            {
                ExitResolving(contractType);            
            }
        }

        private void EnterResolving(Type contractType)
        {
            if (!_resolvingTypes.Add(contractType))
                throw new Exception($"Circular dependency detected while resolving type {contractType}. " +
                    $"Dependencies stack: {string.Join(" ← ", _resolvingTypes)} ← {contractType}.");
        }

        private void ExitResolving(Type contractType) => _resolvingTypes.RemoveLast(contractType);

        private Binding FindBinding(Type contractType)
        {
            var currentContainer = this;
            var binding = default(Binding);

            while (!currentContainer?._bindings.TryGetValue(contractType, out binding) ?? false)
                currentContainer = currentContainer._parentContainer;
                
            return binding;
        }

        internal void ResolveNonLazyBindings()
        {
            foreach (var bindingType in _bindingsTypes)
            {
                var binding = _bindings[bindingType];

                if (!binding.IsNonLazy)
                    continue;

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

        internal void CallEntryPoints()
        {
            foreach (var bindingType in _bindingsTypes)
            {
                var binding = _bindings[bindingType];

                if (!binding.IsNonLazy || !binding.IsEntryPoint)
                    continue;

                ((IEntryPoint)binding.CachedInstance).Run();
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
                var parameterInstance = Resolve(parameter.ParameterType);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            methodInjectionData.methodInfo.Invoke(instance, parametersInstances);
        }

        public void Inject(IEnumerable<object> instances)
        {
            foreach (var instance in instances)
                Inject(instance);
        }

        public void AddToInjectionQueue(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Instance can not be null.");

            if (!_injectQueue.Add(instance))
                throw new ArgumentException("Instance already added to inject queue.", nameof(instance));
        }

        public void AddToInjectionQueue(params object[] instances)
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances), "Instances array can not be null.");

            foreach (object instance in instances)
                AddToInjectionQueue(instance);
        }

        internal void InjectQueuedInstances()
        {
            while (_injectQueue.Count > 0)
                Inject(_injectQueue.PopFirst());
        }

        public T Instantiate<T>() => (T)Instantiate(typeof(T));

        public object Instantiate(Type concreteType)
        {
            var constructorInjectionData = ReflectionCache.GetConstructorInjectionData(concreteType);
            var parametersInstances = new object[constructorInjectionData.parametersInfo.Length];

            foreach (var parameter in constructorInjectionData.parametersInfo)
            {
                var parameterInstance = Resolve(parameter.ParameterType);
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

        public void Dispose()
        {
        }
    }
}