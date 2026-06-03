using System;
using Uniject.Bindings;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewGameObjectGetter : InstanceGetter
    {
        public FromNewComponentOnNewGameObjectGetter(Container container, Type concreteType) : base(container)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromNewComponentOnNewGameObjectGetter)} " + 
                    "getter must be a Component, but it is not.");
        }

        public override object GetObject(Type concreteType)
        {
            var component = new GameObject(concreteType.Name).AddComponent(concreteType);
            _container.Inject(component);
            return component;
        }
    }
}