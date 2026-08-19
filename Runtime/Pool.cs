using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Uniject.Bindings.Pools;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject
{
    public abstract class Pool : IDisposable
    {
        public int InitialSize { get; protected set; }
        public int MaxSize { get; protected set; }
        public ExpandType ExpandType { get; protected set; }
        public bool WithoutGameObjectActivation { get; protected set; }


        public abstract void Clear();

        public abstract void Dispose();

        public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
        {
            public static ReferenceEqualityComparer<T> Instance { get; } = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);

            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class Pool<TResult> : Pool, IPool<TResult> where TResult : class
    {
        private Factory<TResult> _factory;
        private InstanceGetter _instanceGetter;
        private Type _resultContractType;
        private Type _resultConcreteType;
        protected HashSet<TResult> _spawnedInstancesSet;
        protected List<TResult> _despawnedInstances;
        protected HashSet<TResult> _despawnedInstancesSet;

        public int InstanceCount => _spawnedInstancesSet.Count + _despawnedInstances.Count;

        public void Initialize(InstanceGetter instanceGetter, Type resultConcreteType, int initialSize, int maxSize, 
            ExpandType expandType, bool withoutGameObjectActivation)
        {
            Initialize(
                instanceGetter,
                typeof(TResult),
                resultConcreteType,
                initialSize,
                maxSize,
                expandType,
                withoutGameObjectActivation);
        }

        internal void Initialize(
            InstanceGetter instanceGetter,
            Type resultContractType,
            Type resultConcreteType,
            int initialSize,
            int maxSize,
            ExpandType expandType,
            bool withoutGameObjectActivation)
        {
            InitialSize = initialSize;
            MaxSize = maxSize;
            ExpandType = expandType;
            WithoutGameObjectActivation = withoutGameObjectActivation;
            _instanceGetter = instanceGetter;
            _resultContractType = resultContractType;
            _resultConcreteType = resultConcreteType;
            
            _factory = new Factory<TResult>();
            _factory.Construct(_instanceGetter, _resultContractType, _resultConcreteType);

            var initialCapacity = MaxSize == -1 ? InitialSize : Mathf.Min(InitialSize, MaxSize);

            var comparer = ReferenceEqualityComparer<TResult>.Instance;
            _spawnedInstancesSet = new HashSet<TResult>(comparer);
            _despawnedInstances = new List<TResult>(initialCapacity);
            _despawnedInstancesSet = new HashSet<TResult>(initialCapacity, comparer);

            for (int i = 0; i < initialCapacity; i++)
                CreateDespawnedInstance();
        }

        private TResult CreateDespawnedInstance()
        {
            var instance = _factory.Create();

            if (instance is null)
                throw new InvalidOperationException("Factory returned a null instance.");

            if (_spawnedInstancesSet.Contains(instance) || _despawnedInstancesSet.Contains(instance))
                throw new InvalidOperationException("Instance is already in the pool.");

            _despawnedInstances.Add(instance);
            _despawnedInstancesSet.Add(instance);
            
            Reset(instance);
            TrySetActiveForGameObject(instance, false);

            return instance;
        }

        private void TrySetActiveForGameObject(TResult instance, bool active)
        {
            if (WithoutGameObjectActivation)
                return;

            if (instance is GameObject gameObject)
                gameObject.SetActive(active);
            else if (instance is Component component)
                component.gameObject.SetActive(active);
        }

        public TResult Spawn()
        {
            if (_despawnedInstances.Count == 0)
            {
                if (MaxSize != -1 && InstanceCount >= MaxSize)
                    throw new InvalidOperationException("Pool has reached its maximum size.");

                var nextCount = ExpandType switch
                {
                    ExpandType.ByOne => InstanceCount + 1,
                    ExpandType.ByDoubling => InstanceCount == 0 ? 1 : InstanceCount * 2,
                    _ => throw new NotImplementedException()
                };

                nextCount = MaxSize == -1 ? nextCount : Mathf.Min(nextCount, MaxSize);

                for (int i = InstanceCount; i < nextCount; i++)
                    CreateDespawnedInstance();
            }

            var instance = _despawnedInstances[_despawnedInstances.Count - 1];
            
            _despawnedInstances.RemoveAt(_despawnedInstances.Count - 1);
            _despawnedInstancesSet.Remove(instance);
            _spawnedInstancesSet.Add(instance);

            TrySetActiveForGameObject(instance, true);

            return instance;
        }

        public void Despawn(TResult instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Despawning instance can not be a null.");
                
            if (!_spawnedInstancesSet.Contains(instance))
                throw new InvalidOperationException("Despawning instance is not from the pool.");

            if (_despawnedInstancesSet.Contains(instance))
                throw new InvalidOperationException("Despawning instance is already in the pool.");

            _spawnedInstancesSet.Remove(instance);
            _despawnedInstances.Add(instance);
            _despawnedInstancesSet.Add(instance);
            
            Reset(instance);
            TrySetActiveForGameObject(instance, false);
        }

        public void Adopt(TResult instance)
        {
            if (instance == null || instance is UnityEngine.Object unityObject && unityObject == null)
                throw new ArgumentNullException(nameof(instance));

            if (_spawnedInstancesSet.Contains(instance) || _despawnedInstancesSet.Contains(instance))
                throw new InvalidOperationException("Instance is already tracked by the pool.");

            if (MaxSize != -1 && InstanceCount >= MaxSize)
                throw new InvalidOperationException("Pool has reached its maximum size.");

            Reset(instance);
            TrySetActiveForGameObject(instance, false);

            _despawnedInstances.Add(instance);
            _despawnedInstancesSet.Add(instance);
        }

        protected virtual void Reset(TResult instance) { }

        override public void Clear()
        {
            foreach (var instance in _spawnedInstancesSet)
            {
                if (instance is Component component)
                    UnityEngine.Object.Destroy(component.gameObject);
                else if (instance is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            }

            foreach (var instance in _despawnedInstances)
            {
                if (instance is Component component)
                    UnityEngine.Object.Destroy(component.gameObject);
                else if (instance is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            }

            _spawnedInstancesSet.Clear();
            _spawnedInstancesSet.Clear();
            _despawnedInstances.Clear();
            _despawnedInstancesSet.Clear();
        }

        public override void Dispose() => Clear();
    }
}
