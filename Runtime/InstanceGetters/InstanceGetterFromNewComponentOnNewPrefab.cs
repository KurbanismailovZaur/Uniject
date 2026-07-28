using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromNewComponentOnNewPrefab : InstanceGetter
    {
        private readonly GameObject _prefab;

        private InstanceGetterFromNewComponentOnNewPrefab(Container container, object prefab, Type concreteType) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(InstanceGetterFromNewComponentOnNewPrefab)} can not be null.");

            if (!TypeValidator.TypeIsComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromNewComponentOnNewPrefab)} " +
                    "must be a Component, but it is not.");
        }

        public InstanceGetterFromNewComponentOnNewPrefab(Container container, GameObject prefab, Type concreteType) 
            : this(container, (object)prefab, concreteType)
        {
            _prefab = prefab;
        }

        public InstanceGetterFromNewComponentOnNewPrefab(Container container, Component prefab, Type concreteType) 
            : this(container, (object)prefab, concreteType)
        {
            _prefab = prefab.gameObject;
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var cloned = Container.Instantiate(_prefab);
            var component = Container.AddComponent(cloned, concreteType);

            SetGameObjectNameAndParent(component, createOptions);
            return component;
        }
    }
}
