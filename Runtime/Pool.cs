using System;
using System.Collections.Generic;
using Uniject.Bindings.Pools;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject
{
    public abstract class PoolBase : IDisposable
    {
        public int InitialSize { get; protected set; }
        public int MaxSize { get; protected set; }
        public ExpandType ExpandType { get; protected set; }

        public abstract void Clear();

        public abstract void Dispose();
    }

    public abstract class PoolBase<TObjectType> : PoolBase where TObjectType : class
    {
        protected HashSet<TObjectType> _spawnedInstancesSet;
        protected List<TObjectType> _despawnedInstances;
        protected HashSet<TObjectType> _despawnedInstancesSet;
        public int InstanceCount => _spawnedInstancesSet.Count + _despawnedInstances.Count;
    }

    public class Pool<TResult> : PoolBase<TResult>, IPool<TResult> where TResult : class
    {
        private Factory<TResult> _factory;
        private InstanceGetter _instanceGetter;
        private Type _resultConcreteType;

        internal void Construct(InstanceGetter instanceGetter, Type resultConcreteType, int initialSize,
            int maxSize, ExpandType expandType)
        {
            InitialSize = initialSize;
            MaxSize = maxSize;
            ExpandType = expandType;
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        internal void Initialize()
        {
            _factory = new Factory<TResult>();
            _factory.Construct(_instanceGetter, _resultConcreteType);

            var initialCapacity = MaxSize == -1 ? InitialSize : Mathf.Min(InitialSize, MaxSize);
            _spawnedInstancesSet = new HashSet<TResult>();
            _despawnedInstances = new List<TResult>(initialCapacity);
            _despawnedInstancesSet = new HashSet<TResult>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
                CreateDespawnedInstance();
        }

        private TResult CreateDespawnedInstance()
        {
            var instance = _factory.Create();

            if (_despawnedInstancesSet.Contains(instance))
                throw new InvalidOperationException("Instance is already in the pool.");

            _despawnedInstances.Add(instance);
            _despawnedInstancesSet.Add(instance);
            
            Reset(instance);
            TrySetActiveForGameObject(instance, false);

            return instance;
        }

        private void TrySetActiveForGameObject(TResult instance, bool active)
        {
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

        protected virtual void Reset(TResult instance) { }

        override public void Clear()
        {
            foreach (var instance in _spawnedInstancesSet)
            {
                if (instance is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            }

            foreach (var instance in _despawnedInstances)
            {
                if (instance is UnityEngine.Object unityObject)
                    UnityEngine.Object.Destroy(unityObject);
            }

            _spawnedInstancesSet.Clear();
            _spawnedInstancesSet.Clear();
            _despawnedInstances.Clear();
            _despawnedInstancesSet.Clear();
        }

        public override void Dispose() => Clear();
    }

    public class Pool<TParam, TResult> : PoolBase, IPool<TParam, TResult> where TParam : class where TResult : class
    {
        private Factory<TParam, TResult> _factory;
        private InstanceGetterWithParameter<TParam> _instanceGetter;
        private Type _resultConcreteType;

        internal void Construct(InstanceGetterWithParameter<TParam> instanceGetter, Type resultConcreteType, int initialSize, int maxSize, ExpandType expandType)
        {
            InitialSize = initialSize;
            MaxSize = maxSize;
            ExpandType = expandType;
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        internal void Initialize()
        {
            _factory = new Factory<TParam, TResult>();
            _factory.Construct(_instanceGetter, _resultConcreteType);
        }

        public TResult Spawn(TParam origin) => _factory.Create(origin);

        public void Despawn(TResult instance)
        {
            if (instance is UnityEngine.Object unityObject)
                UnityEngine.Object.Destroy(unityObject);
        }

        public override void Clear()
        {
            
        }

        override public void Dispose()
        {
            
        }
    }
}