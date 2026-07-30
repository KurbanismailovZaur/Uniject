using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromComponentInParents : InstanceGetter
    {
        public InstanceGetterFromComponentInParents(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromComponentInParents)} " +
                    "must be a Component or an interface.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            if (context.ConsumerInstance is not MonoBehaviour consumer || consumer == null)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromComponentInParents)} can only be used during method injection " +
                    "into a live MonoBehaviour.");

            var currentTransform = consumer.transform;

            while (currentTransform != null)
            {
                var component = currentTransform.gameObject.GetComponent(concreteType);

                if (component != null)
                    return component;

                currentTransform = currentTransform.parent;
            }

            throw new InvalidOperationException(
                $"{nameof(InstanceGetterFromComponentInParents)} could not find a component assignable to type " +
                $"{concreteType} on GameObject '{consumer.gameObject.name}' or any of its parents " +
                $"for consumer {consumer.GetType()}.");
        }
    }
}
