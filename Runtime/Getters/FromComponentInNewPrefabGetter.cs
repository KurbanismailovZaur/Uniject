using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter : InstanceGetter
    {
        private readonly object _prefab;

        public FromComponentInNewPrefabGetter(Container container, object prefab, Type concreteType) : base(container)
        {
            if (prefab == null) 
                throw new ArgumentNullException(nameof(prefab), 
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter)} getter can not be null.");

            if (prefab is Component component)
            {
                if (component.GetType() == concreteType)
                    _prefab = prefab;
                else
                {
                    if (component.TryGetComponent(concreteType, out var concreteComponent))
                        _prefab = concreteComponent;
                    else
                        throw new ArgumentException($"Gameobject \"{component.name}\" for {nameof(FromComponentInNewPrefabGetter)} does not have a component of type {concreteType.Name}.");
                }
            }
            else if (prefab is GameObject gameObject)
            {
                if (gameObject.TryGetComponent(concreteType, out component))
                    _prefab = component;
                else
                    throw new ArgumentException($"GameObject \"{gameObject.name}\" for {nameof(FromComponentInNewPrefabGetter)} does not have a component of type {concreteType.Name}.");
            }
            else
                throw new ArgumentException($"Prefab for {nameof(FromComponentInNewPrefabGetter)} must be a Component or a GameObject, but it is not.");
        }

        public override object GetObject(Type concreteType) => _container.InstantiatePrefab(_prefab as Component);

        // TODO: Короче надо чекать какой тип в To и приводить в _prefab присваивать этот экземпляр, а затем в
        // методе GetObject вызывать Instantiate(gameobject) и в зависимости от того что лежит в _prefab,
        // возвращать либо компонент, либо геймобджект. Также надо переделать _container.InstantiatePrefab чтобы 
        // основным был тот который принимает gameobject.
    }
}