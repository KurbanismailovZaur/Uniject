using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter<TConcrete> : InstanceGetter
    {
        private readonly Component _prefab;

        private FromComponentInNewPrefabGetter(Container container, object prefab) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter can not be null.");
        }

        public FromComponentInNewPrefabGetter(Container container, GameObject prefab, Type concreteType) : this(container, prefab)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter must be a Component, but it is not.",
                    nameof(concreteType));

            _prefab = prefab.GetComponent(concreteType);

            if (_prefab == null)
                throw new ArgumentException(
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter must have a component of type {concreteType}.",
                    nameof(prefab));
        }

        public FromComponentInNewPrefabGetter(Container container, TConcrete prefab) : this(container, (object)prefab)
        {
            if (prefab is not Component component)
                throw new ArgumentException(
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter must be a Component.",
                    nameof(prefab));

            _prefab = component;
        }
        
        public override object GetInstance(Type concreteType) => _container.Instantiate(_prefab);
    }
}