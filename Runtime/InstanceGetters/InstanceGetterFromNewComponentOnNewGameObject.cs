using System;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromNewComponentOnNewGameObject : InstanceGetter
    {
        public InstanceGetterFromNewComponentOnNewGameObject(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeCanBeAddedAsComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromNewComponentOnNewGameObject)} must be a non-abstract Component.");
        }

        public override object GetInstance(Type concreteType)
        {
            var gameObject = new GameObject(concreteType.Name);
            var component = _container.AddComponent(gameObject, concreteType);

            return component;
        }
    }
}