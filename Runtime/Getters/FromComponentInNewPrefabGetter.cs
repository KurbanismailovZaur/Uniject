using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter<T> : InstanceGetter
    {
        private readonly T _prefab;

        public FromComponentInNewPrefabGetter(Container container, T prefab) : base(container)
        {
            if (prefab == null) 
                throw new ArgumentNullException(nameof(prefab), 
                    $"Prefab of type {typeof(T)} for FromComponentInNewPrefab getter can not be null.");

            if (prefab is not Component)
                throw new ArgumentException($"Prefab for FromComponentInNewPrefab getter must be a Component, but it is not.");

            _prefab = prefab;
        }

        public override object GetObject(Type concreteType)
        {
            return _container.InstantiatePrefab(_prefab as Component);
        }
    }
}