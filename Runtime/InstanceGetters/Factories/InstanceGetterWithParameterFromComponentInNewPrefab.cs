using System;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters.Factories
{
    // public class InstanceGetterWithParameterFromComponentInNewPrefab<TParam> : InstanceGetterWithParameter<TParam>
    // {
    //     private readonly Component _prefab;

    //     public InstanceGetterWithParameterFromComponentInNewPrefab(Container container, Type concreteType) : base(container)
    //     {
    //         if (TypeValidator.TypeIsNotInterfaceOrComponent(concreteType))
    //             throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromComponentInNewPrefab)} must " + 
    //                 "be a Component or an interface.", nameof(concreteType));

    //         _prefab = prefab.GetComponent(concreteType);

    //         if (_prefab == null)
    //             throw new ArgumentException($"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} must have a " +
    //                 $"component assignable to type {concreteType}.", nameof(prefab));
    //     }

    //     public InstanceGetterFromComponentInNewPrefab(Container container, Component prefab, Type concreteType) 
    //         : this(container, prefab)
    //     {
    //         if (TypeValidator.TypeIsNotInterfaceOrComponent(concreteType))
    //             throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromComponentInNewPrefab)} must " + 
    //                 "be a Component or an interface.", nameof(concreteType));

    //         _prefab = prefab.GetComponent(concreteType);

    //         if (_prefab == null)
    //             throw new ArgumentException($"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} must have a " + 
    //                 $"component assignable to type {concreteType}.", nameof(prefab));
    //     }

    //     public override object GetInstance(Type concreteType) => _container.Instantiate(_prefab);
    // }
}