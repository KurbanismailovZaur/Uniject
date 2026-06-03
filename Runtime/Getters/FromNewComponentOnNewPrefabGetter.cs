using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewPrefabGetter : InstanceGetter
    {
        private readonly GameObject _prefab;

        public FromNewComponentOnNewPrefabGetter(Container container, GameObject prefab, Type concreteType) : base(container)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromNewComponentOnNewPrefabGetter)} " + 
                    "getter must be a Component, but it is not.");

            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(FromNewComponentOnNewPrefabGetter)} getter can not be null.");

            _prefab = prefab;
        }

        public override object GetObject(Type concreteType)
        {
            var cloned = _container.Instantiate(_prefab);

            var script = cloned.AddComponent(concreteType);
            _container.Inject(script);

            return script;
        }
    }
}