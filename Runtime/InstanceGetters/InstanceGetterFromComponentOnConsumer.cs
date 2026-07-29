using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromComponentOnConsumer : InstanceGetter
    {
        public InstanceGetterFromComponentOnConsumer(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromComponentOnConsumer)} " +
                    "must be a Component or an interface.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            if (context.ConsumerInstance is not MonoBehaviour consumer || consumer == null)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromComponentOnConsumer)} can only be used during method injection " +
                    "into a live MonoBehaviour.");

            var component = consumer.gameObject.GetComponent(concreteType);

            if (component == null)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromComponentOnConsumer)} could not find a component assignable to type " +
                    $"{concreteType} on GameObject '{consumer.gameObject.name}' of consumer {consumer.GetType()}.");

            return component;
        }
    }
}
