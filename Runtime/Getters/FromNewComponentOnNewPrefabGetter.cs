using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewPrefabGetter<TConcrete> : InstanceGetter
    {
        private readonly GameObject _prefab;

        private FromNewComponentOnNewPrefabGetter(Container container, object prefab, Type concreteType) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(FromNewComponentOnNewPrefabGetter<TConcrete>)} getter can not be null.");

            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromNewComponentOnNewPrefabGetter<TConcrete>)} " +
                    "getter must be a Component, but it is not.");
        }

        public FromNewComponentOnNewPrefabGetter(Container container, GameObject prefab, Type concreteType) 
            : this(container, (object)prefab, concreteType)
        {
            _prefab = prefab;
        }

        public FromNewComponentOnNewPrefabGetter(Container container, TConcrete prefab, Type concreteType) 
            : this(container, (object)prefab, concreteType)
        {
            if (prefab is not Component component)
                throw new ArgumentException(
                    $"Prefab for {nameof(FromNewComponentOnNewPrefabGetter<TConcrete>)} getter must be a Component.",
                    nameof(prefab));

            _prefab = component.gameObject;
        }

        public override object GetInstance(Type concreteType)
        {
            var cloned = _container.Instantiate(_prefab);
            var component = _container.AddComponent(cloned, concreteType);

            return component;
        }
    }
}
