using System;
using System.Collections.Generic;
using Uniject.Bindings.Pools;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject
{
    public abstract class PoolBase
    {
        protected int _initialSize;
        protected int _maxSize;
        protected ExpandType _expandType;
    }

    public abstract class PoolBase<TObjectType> : PoolBase
    {
        protected List<TObjectType> _availableInstances;
        protected HashSet<TObjectType> _availableInstancesSet;
    }

    public class Pool<TResult> : PoolBase<TResult>, IPool<TResult>
    {
        private Factory<TResult> _factory;
        private InstanceGetter _instanceGetter;
        private Type _resultConcreteType;

        internal void Construct(InstanceGetter instanceGetter, Type resultConcreteType, int initialSize, int maxSize, ExpandType expandType)
        {
            _initialSize = initialSize;
            _maxSize = maxSize;
            _expandType = expandType;
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        internal void Initialize()
        {
            _factory = new Factory<TResult>();
            _factory.Construct(_instanceGetter, _resultConcreteType);

            var capacity = _maxSize == -1 ? _initialSize : _maxSize;
            _availableInstances = new List<TResult>(capacity);
            _availableInstancesSet = new HashSet<TResult>(capacity);

            var initialSize = Mathf.Min(_initialSize, _maxSize);

            for (int i = 0; i < initialSize; i++)
            {
                var instance = _factory.Create();
                Despawn(instance);
            }
        }

        public TResult Spawn()
        {
            if (_availableInstances.Count > 0)
            {
                var instance = _availableInstances[_availableInstances.Count - 1];
                _availableInstances.RemoveAt(_availableInstances.Count - 1);
                _availableInstancesSet.Remove(instance);

                if (instance is GameObject gameObject)
                    gameObject.SetActive(true);
                else if (instance is Component component)
                    component.gameObject.SetActive(true);

                return instance;
            }

            return _factory.Create();
        }

        public void Despawn(TResult instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), "Despawning instance can not be a null.");

            if (_availableInstancesSet.Contains(instance))
                throw new InvalidOperationException("Despawning instance is already in the pool.");

            if (_maxSize == -1 || _availableInstances.Count < _maxSize)
            {
                Reset(instance);

                if (instance is GameObject gameObject)
                    gameObject.SetActive(false);
                else if (instance is Component component)
                    component.gameObject.SetActive(false);

                if (_availableInstances.Count == _availableInstances.Capacity)
                {
                    var nextCapacity = _expandType switch
                    {
                        ExpandType.ByDoubling => _availableInstances.Capacity * 2,
                        ExpandType.ByOne => _availableInstances.Capacity + 1,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    _availableInstances.Capacity = _maxSize == -1 ? nextCapacity : Mathf.Min(nextCapacity, _maxSize);
                }

                _availableInstances.Add(instance);
                _availableInstancesSet.Add(instance);
            }
            else
            {
                if (instance is GameObject gameObject)
                    UnityEngine.Object.Destroy(gameObject);
                else if (instance is Component component)
                    UnityEngine.Object.Destroy(component.gameObject);
            }
        }

        protected virtual void Reset(TResult instance) { }
    }

    public class Pool<TParam, TResult> : PoolBase, IPool<TParam, TResult>
    {
        private Factory<TParam, TResult> _factory;
        private InstanceGetterWithParameter<TParam> _instanceGetter;
        private Type _resultConcreteType;

        internal void Construct(InstanceGetterWithParameter<TParam> instanceGetter, Type resultConcreteType, int initialSize, int maxSize, ExpandType expandType)
        {
            _initialSize = initialSize;
            _maxSize = maxSize;
            _expandType = expandType;
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
    }
}