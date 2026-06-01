using System;
using Uniject.Bindings;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewGameObjectGetter : InstanceGetter
    {
        public FromNewComponentOnNewGameObjectGetter(Type concreteType, Container container) : base(container)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for FromNewComponentOnNewGameObject getter must be a Component, but it is not.");
        }

        public override object GetObject(Type concreteType)
        {
            var component = new GameObject(concreteType.Name).AddComponent(concreteType);
            _container.Inject(component);
            return component;
        }
    }
}