using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Uniject.Bindings;
using Uniject.Bindings.Factories;
using Uniject.Bindings.Pools;
using Uniject.Collections;
using Uniject.Components;
using Uniject.Contexts;
using Uniject.Exceptions;
using Uniject.Lifecycle;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject
{
    public class Container : IObjectBuilder, IDisposable
    {
        private enum DisposalState
        {
            Alive,
            Disposing,
            Disposed
        }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public static ReferenceEqualityComparer<T> Instance { get; } = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);

            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }

        private Container _parentContainer;
        private Container _ownerContainer;
        private Transform _parentTransformForGameObjects;

        private readonly Dictionary<Type, Binding> _bindings = new();
        private readonly List<Type> _bindingsTypes = new();
        private readonly OrderedSet<Type> _resolvingTypes = new();
        private readonly OrderedSet<object> _injectQueue = new();
        private readonly Stack<IDisposable> _disposables = new();
        private readonly HashSet<IDisposable> _disposablesSet =
            new(ReferenceEqualityComparer<IDisposable>.Instance);
        private readonly HashSet<IDisposable> _disposedDisposableHistory =
            new(ReferenceEqualityComparer<IDisposable>.Instance);
        private readonly List<Container> _ownedChildContainers = new();
        private readonly HashSet<Container> _ownedChildContainersSet =
            new(ReferenceEqualityComparer<Container>.Instance);
        private DisposalState _disposalState;

        public Transform ParentTransformForGameObjects
        {
            get => _parentTransformForGameObjects;
            set
            {
                ThrowIfDisposed();
                _parentTransformForGameObjects = value;
            }
        }
        public Context Context { get; private set; }

        public bool IsBuilded { get; private set; }

        public Container(Container parentContainer = null, Transform parentTransformForGameObjects = null, Context context = null)
        {
            SetParentContainer(parentContainer);
            ParentTransformForGameObjects = parentTransformForGameObjects;
            Context = context;

            Bind<Container>().FromInstance(this).AsCached();
            Bind<IObjectBuilder>().FromInstance(this).AsCached();
        }

        public void SetParentContainer(Container parentContainer)
        {
            ThrowIfDisposed();
            _parentContainer = parentContainer;
        }

        internal void RegisterDisposable(object instance, Type contractType)
        {
            ThrowIfDisposed();

            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));

            if (instance is not IDisposable disposable)
                throw new InvalidOperationException(
                    $"Instance {(instance == null ? "null" : $"of type {instance.GetType()}")} " +
                    $"registered for contract {contractType} " +
                    $"must implement {typeof(IDisposable)}.");

            if (ReferenceEquals(disposable, this))
                throw new InvalidOperationException(
                    $"Container can not register itself for disposal for contract {contractType}.");

            if (_disposablesSet.Add(disposable))
                _disposables.Push(disposable);
        }

        internal void RegisterOwnedChildContainer(Container childContainer)
        {
            ThrowIfDisposed();

            if (childContainer == null)
                throw new ArgumentNullException(nameof(childContainer));

            if (ReferenceEquals(childContainer, this))
                throw new ArgumentException("Container can not own itself.", nameof(childContainer));

            if (childContainer._ownerContainer != null &&
                !ReferenceEquals(childContainer._ownerContainer, this))
            {
                throw new InvalidOperationException("Container is already owned by another container.");
            }

            if (_ownedChildContainersSet.Add(childContainer))
            {
                _ownedChildContainers.Add(childContainer);
                childContainer._ownerContainer = this;
            }
        }

        internal void UnregisterOwnedChildContainer(Container childContainer)
        {
            if (childContainer == null)
                throw new ArgumentNullException(nameof(childContainer));

            if (!_ownedChildContainersSet.Remove(childContainer))
                return;

            for (var i = _ownedChildContainers.Count - 1; i >= 0; i--)
            {
                if (!ReferenceEquals(_ownedChildContainers[i], childContainer))
                    continue;

                _ownedChildContainers.RemoveAt(i);
                childContainer._ownerContainer = null;
                childContainer._disposedDisposableHistory.Clear();
                break;
            }
        }

        internal void ThrowIfDisposed()
        {
            if (_disposalState != DisposalState.Alive)
                throw new ObjectDisposedException(nameof(Container));
        }

        internal IReadOnlyList<Container> GetSelfAndParents()
        {
            ThrowIfDisposed();

            var visitedContainers = new HashSet<Container>();
            var containers = new List<Container>();
            var currentContainer = this;

            while (currentContainer != null)
            {
                if (!visitedContainers.Add(currentContainer))
                    throw new InvalidOperationException("A cycle was detected in the container hierarchy.");

                containers.Add(currentContainer);
                currentContainer = currentContainer._parentContainer;
            }

            return containers;
        }

        internal bool IsStrictDescendantOf(Container ancestor)
        {
            ThrowIfDisposed();

            if (ancestor == null)
                throw new ArgumentNullException(nameof(ancestor));

            var visitedContainers = new HashSet<Container>();
            var currentContainer = _parentContainer;
            var isDescendant = false;

            while (currentContainer != null)
            {
                if (!visitedContainers.Add(currentContainer))
                    throw new InvalidOperationException("A cycle was detected in the container hierarchy.");

                if (ReferenceEquals(currentContainer, ancestor))
                    isDescendant = true;

                currentContainer = currentContainer._parentContainer;
            }

            return isDescendant;
        }

        public BindingToBuilder<TContract> Bind<TContract>() => new(this, CreateBinding(typeof(TContract)));

        public BindingToTypeToBuilder Bind(Type contractType) => new(this, CreateBinding(contractType));

        private BindingToType CreateBinding(Type contractType)
        {
            ThrowIfDisposed();

            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));

            if (_bindings.ContainsKey(contractType))
                throw new InvalidOperationException($"Type {contractType} is already bound.");

            var binding = new BindingToType(this, contractType);
            _bindings[contractType] = binding;
            _bindingsTypes.Add(contractType);
            return binding;
        }

        public BindingToTypeCachedNonLazyBuilder BindInstance<T>(T instance)
        {
            ThrowIfDisposed();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return Bind<T>().FromInstance(instance).AsCached().NonLazy();
        }

        public void BindInstances(params object[] instances)
        {
            ThrowIfDisposed();

            if (instances == null)
                throw new ArgumentNullException(nameof(instances));

            foreach (var instance in instances)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instances));

                Bind(instance.GetType()).FromInstance(instance);
            }
        }

        public BindingToFactoryToBuilder<TResult, TFactory> BindFactory<TResult, TFactory>()
            where TFactory : Factory<TResult>, new()
        {
            return new(this, CreateBindingToFactory<TResult, TFactory>(typeof(TResult), typeof(TFactory)));
        }

        private BindingToFactory<TResult, TFactory> CreateBindingToFactory<TResult, TFactory>(Type resultType, Type factoryType)
            where TFactory : Factory<TResult>, new()
        {
            ThrowIfDisposed();

            if (_bindings.ContainsKey(factoryType))
                throw new InvalidOperationException($"Type {factoryType} is already bound.");

            var binding = new BindingToFactory<TResult, TFactory>(this, resultType, factoryType);
            _bindings[factoryType] = binding;
            _bindingsTypes.Add(factoryType);
            return binding;
        }

        public BindingToFactoryWithParameterToBuilder<TParam, TResult, TFactory> BindFactory<TParam, TResult, TFactory>()
            where TFactory : Factory<TParam, TResult>, new()
        {
            return new(this, CreateBindingToFactoryWithParameter<TParam, TResult, TFactory>(typeof(TParam), typeof(TResult), typeof(TFactory)));
        }

        private BindingToFactoryWithParameter<TParam, TResult, TFactory> CreateBindingToFactoryWithParameter<TParam, TResult, TFactory>(Type paramType, Type resultType, Type factoryType)
            where TFactory : Factory<TParam, TResult>, new()
        {
            ThrowIfDisposed();

            if (_bindings.ContainsKey(factoryType))
                throw new InvalidOperationException($"Type {factoryType} is already bound.");

            var binding = new BindingToFactoryWithParameter<TParam, TResult, TFactory>(this, paramType, resultType, factoryType);
            _bindings[factoryType] = binding;
            _bindingsTypes.Add(factoryType);
            return binding;
        }

        public BindingToPoolWithInitialSizeBuilder<TResult, TPool> BindPool<TResult, TPool>()
            where TResult : class
            where TPool : Pool<TResult>, new()
        {
            return new(this, CreateBindingToPool<TResult, TPool>(typeof(TResult), typeof(TPool)));
        }

        private BindingToPool<TResult, TPool> CreateBindingToPool<TResult, TPool>(Type resultType, Type poolType)
            where TResult : class
            where TPool : Pool<TResult>, new()
        {
            ThrowIfDisposed();

            if (_bindings.ContainsKey(poolType))
                throw new InvalidOperationException($"Type {poolType} is already bound.");

            var binding = new BindingToPool<TResult, TPool>(this, resultType, poolType);
            _bindings[poolType] = binding;
            _bindingsTypes.Add(poolType);
            return binding;
        }

        public T Resolve<T>() => (T)Resolve(typeof(T));

        public object Resolve(Type contractType)
        {
            ThrowIfDisposed();
            return Resolve(contractType, InjectContext.CreateRoot(this, contractType));
        }

        internal object Resolve(Type contractType, InjectContext context)
        {
            ThrowIfDisposed();

            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));

            context.EnsureIsValid(nameof(context));
            EnterResolving(contractType);

            try
            {
                var binding = FindBinding(contractType);

                if (binding == null)
                    throw new NoBindingFoundException($"No binding found for type {contractType}. " +
                        $"Dependencies stack: {string.Join(" ← ", _resolvingTypes)}.");

                binding.Container.ThrowIfDisposed();
                return binding.GetInstance(context.WithContainer(binding.Container));
            }
            finally
            {
                ExitResolving(contractType);
            }
        }

        public (T, bool resolved) TryResolve<T>()
        {
            var instance = TryResolve(typeof(T));

            return instance switch
            {
                null => (default, false),
                _ => ((T)instance, true)
            };
        }

        public object TryResolve(Type contractType)
        {
            ThrowIfDisposed();

            try
            {
                return Resolve(contractType);
            }
            catch (NoBindingFoundException)
            {
                return null;
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

            while (currentContainer != null)
            {
                currentContainer.ThrowIfDisposed();

                if (currentContainer._bindings.TryGetValue(contractType, out var binding))
                    return binding;

                currentContainer = currentContainer._parentContainer;
            }

            return null;
        }

        internal void ResolveNonLazyBindings()
        {
            ThrowIfDisposed();

            foreach (var bindingType in _bindingsTypes)
            {
                var bindingBase = _bindings[bindingType];

                if (bindingBase is not BindingToType binding || !binding.IsNonLazy)
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

        internal void RunEntryPoints()
        {
            ThrowIfDisposed();

            foreach (var bindingType in _bindingsTypes)
            {
                var bindingBase = _bindings[bindingType];

                if (bindingBase is not BindingToType binding || !binding.IsNonLazy || !binding.IsEntryPoint)
                    continue;

                ((IEntryPoint)binding.CachedInstance).Run();
            }
        }

        public void Inject(object instance)
        {
            ThrowIfDisposed();

            var methodInjectionData = ReflectionCache.GetMethodInjectionData(instance.GetType());

            if (!methodInjectionData.hasInjectMethod)
                return;

            var parametersInstances = new object[methodInjectionData.parametersInfo.Length];

            foreach (var parameter in methodInjectionData.parametersInfo)
            {
                var context = InjectContext.CreateForMethodParameter(this, parameter, instance);
                var parameterInstance = Resolve(parameter.ParameterType, context);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            methodInjectionData.methodInfo.Invoke(instance, parametersInstances);
        }

        public void Inject(IEnumerable<object> instances)
        {
            ThrowIfDisposed();

            foreach (var instance in instances)
                Inject(instance);
        }

        public void AddToInjectionQueue(object instance)
        {
            ThrowIfDisposed();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Instance can not be null.");

            if (!_injectQueue.Add(instance))
                throw new ArgumentException("Instance already added to inject queue.", nameof(instance));
        }

        public void AddToInjectionQueue(params object[] instances)
        {
            ThrowIfDisposed();

            if (instances == null)
                throw new ArgumentNullException(nameof(instances), "Instances array can not be null.");

            foreach (object instance in instances)
                AddToInjectionQueue(instance);
        }

        internal void InjectQueuedInstances()
        {
            ThrowIfDisposed();

            while (_injectQueue.Count > 0)
                Inject(_injectQueue.PopFirst());
        }

        public Context GetNearestContext()
        {
            ThrowIfDisposed();

            var container = this;
            while (container != null)
            {
                if (container.Context != null)
                    return container.Context;

                container = container._parentContainer;
            }

            return null;
        }

        public (Context context, Transform parentTransform) GetInfoAboutNearestParentForGameObjects()
        {
            ThrowIfDisposed();

            var container = this;
            var parentTransform = default(Transform);

            while (container != null && parentTransform == null)
            {
                if (container.ParentTransformForGameObjects != null)
                {
                    parentTransform = container.ParentTransformForGameObjects;
                    break;
                }
                else if (container.Context != null && container.Context is GameObjectContext)
                {
                    parentTransform = container.Context.transform;
                    break;
                }
                else if (container.Context != null && container.Context is SceneContext)
                    break;

                container = container._parentContainer;
            }

            return (container?.Context, parentTransform);
        }


        public void Build()
        {
            ThrowIfDisposed();

            if (IsBuilded)
                return;

            IsBuilded = true;

            ResolveNonLazyBindings();
            InjectQueuedInstances();
            RunEntryPoints();
        }

        public T Instantiate<T>() => (T)Instantiate(typeof(T));

        public T Instantiate<T>(Type concreteType) => (T)Instantiate(concreteType);

        public object Instantiate(Type concreteType)
        {
            ThrowIfDisposed();

            var constructorInjectionData = ReflectionCache.GetConstructorInjectionData(concreteType);
            var parametersInstances = new object[constructorInjectionData.parametersInfo.Length];

            foreach (var parameter in constructorInjectionData.parametersInfo)
            {
                var context = InjectContext.CreateForConstructorParameter(this, parameter, concreteType);
                var parameterInstance = Resolve(parameter.ParameterType, context);
                parametersInstances[parameter.Position] = parameterInstance;
            }

            return constructorInjectionData.constructorInfo.Invoke(parametersInstances);
        }

        public GameObject Instantiate(GameObject prefab)
        {
            ThrowIfDisposed();

            var cloned = UnityEngine.Object.Instantiate(prefab);

            if (cloned.TryGetComponent<InjectTargets>(out var injectionTargets))
                Inject(injectionTargets.Targets);

            return cloned;
        }

        public TComponent Instantiate<TComponent>(TComponent prefab) where TComponent : Component
        {
            return (TComponent)Instantiate(prefab as Component);
        }

        public Component Instantiate(Component prefab)
        {
            ThrowIfDisposed();

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
            ThrowIfDisposed();

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
            ThrowIfDisposed();

            component = component.gameObject.AddComponent(componentType);
            Inject(component);
            return component;
        }

        public void Dispose()
        {
            Dispose(new HashSet<IDisposable>(ReferenceEqualityComparer<IDisposable>.Instance));
        }

        private void Dispose(HashSet<IDisposable> disposedDisposables)
        {
            if (_disposalState != DisposalState.Alive)
            {
                disposedDisposables.UnionWith(_disposedDisposableHistory);
                return;
            }

            _disposalState = DisposalState.Disposing;
            var exceptions = new List<Exception>();

            try
            {
                var ownedChildContainers = _ownedChildContainers.ToArray();
                _ownedChildContainers.Clear();
                _ownedChildContainersSet.Clear();

                for (var i = ownedChildContainers.Length - 1; i >= 0; i--)
                {
                    try
                    {
                        ownedChildContainers[i].Dispose(disposedDisposables);
                    }
                    catch (Exception exception)
                    {
                        AddFlattenedException(exceptions, exception);
                    }
                    finally
                    {
                        ownedChildContainers[i]._ownerContainer = null;
                        ownedChildContainers[i]._disposedDisposableHistory.Clear();
                    }
                }

                while (_disposables.TryPop(out var disposable))
                {
                    _disposablesSet.Remove(disposable);

                    if (!disposedDisposables.Add(disposable))
                        continue;

                    if (_ownerContainer != null)
                        _disposedDisposableHistory.Add(disposable);

                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception exception)
                    {
                        AddFlattenedException(exceptions, exception);
                    }
                }
            }
            finally
            {
                if (_ownerContainer != null)
                    _disposedDisposableHistory.UnionWith(disposedDisposables);

                _ownedChildContainers.Clear();
                _ownedChildContainersSet.Clear();
                _disposables.Clear();
                _disposablesSet.Clear();
                _injectQueue.Clear();
                _bindings.Clear();
                _bindingsTypes.Clear();
                _disposalState = DisposalState.Disposed;
            }

            if (exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        private static void AddFlattenedException(List<Exception> exceptions, Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                    AddFlattenedException(exceptions, innerException);

                return;
            }

            exceptions.Add(exception);
        }
    }
}
