using System;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter : InstanceGetter
    {
        private readonly Component _prefab;

        private FromComponentInNewPrefabGetter(Container container, UnityEngine.Object prefab) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter)} can not be null.");
        }

        public FromComponentInNewPrefabGetter(Container container, GameObject prefab, Type concreteType) 
            : this(container, prefab)
        {
            if (TypeValidator.TypeIsNotInterfaceOrComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromComponentInNewPrefabGetter)} must " + 
                    "be a Component or an interface.", nameof(concreteType));

            _prefab = prefab.GetComponent(concreteType);

            if (_prefab == null)
                throw new ArgumentException($"Prefab for {nameof(FromComponentInNewPrefabGetter)} must have a " +
                    $"component assignable to type {concreteType}.", nameof(prefab));
        }

        public FromComponentInNewPrefabGetter(Container container, Component prefab, Type concreteType) 
            : this(container, prefab)
        {
            if (TypeValidator.TypeIsNotInterfaceOrComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromComponentInNewPrefabGetter)} must " + 
                    "be a Component or an interface.", nameof(concreteType));

            _prefab = prefab.GetComponent(concreteType);

            if (_prefab == null)
                throw new ArgumentException($"Prefab for {nameof(FromComponentInNewPrefabGetter)} must have a " + 
                    $"component assignable to type {concreteType}.", nameof(prefab));
        }

        public override object GetInstance(Type concreteType) => _container.Instantiate(_prefab);
    }
}