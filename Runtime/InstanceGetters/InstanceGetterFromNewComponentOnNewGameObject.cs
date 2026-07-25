using System;
using Uniject.Bindings;
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

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var gameObject = new GameObject(concreteType.Name);
            var component = _container.AddComponent(gameObject, concreteType);

            SetGameObjectNameAndParent(component, createOptions);
            return component;
        }
    }
}
