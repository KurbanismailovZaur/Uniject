using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromNewComponentOn : InstanceGetter
    {
        private readonly GameObject _gameObject;

        public InstanceGetterFromNewComponentOn(Container container, GameObject gameObject, Type concreteType)
            : base(container)
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject),
                    $"Game object for {nameof(InstanceGetterFromNewComponentOn)} can not be null.");

            if (!TypeValidator.TypeCanBeAddedAsComponent(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(InstanceGetterFromNewComponentOn)} " +
                    "must be a non-abstract Component.");

            _gameObject = gameObject;
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            return Container.AddComponent(_gameObject, concreteType);
        }
    }
}
