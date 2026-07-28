using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromComponentInNewPrefab : InstanceGetter
    {
        private readonly Component _prefab;

        private InstanceGetterFromComponentInNewPrefab(Container container, UnityEngine.Object prefab) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} can not be null.");
        }

        public InstanceGetterFromComponentInNewPrefab(Container container, GameObject prefab, Type concreteType) 
            : this(container, prefab)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromComponentInNewPrefab)} must " + 
                    "be a Component or an interface.", nameof(concreteType));

            _prefab = prefab.GetComponent(concreteType);

            if (_prefab == null)
                throw new ArgumentException($"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} must have a " +
                    $"component assignable to type {concreteType}.", nameof(prefab));
        }

        public InstanceGetterFromComponentInNewPrefab(Container container, Component prefab, Type concreteType) 
            : this(container, prefab)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromComponentInNewPrefab)} must " + 
                    "be a Component or an interface.", nameof(concreteType));

            _prefab = prefab.GetComponent(concreteType);

            if (_prefab == null)
                throw new ArgumentException($"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} must have a " + 
                    $"component assignable to type {concreteType}.", nameof(prefab));
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var component = Container.Instantiate(_prefab);
            SetGameObjectNameAndParent(component, createOptions);

            return component;
        }
    }
}
