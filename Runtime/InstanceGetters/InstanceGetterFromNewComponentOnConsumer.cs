using System;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromNewComponentOnConsumer : InstanceGetter
    {
        public InstanceGetterFromNewComponentOnConsumer(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeCanBeAddedAsComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromNewComponentOnConsumer)} " +
                    "must be a non-abstract Component.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            if (context.ConsumerInstance is not MonoBehaviour consumer || consumer == null)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromNewComponentOnConsumer)} can only be used during method injection " +
                    "into a live MonoBehaviour.");

            return context.Container.AddComponent(consumer.gameObject, concreteType);
        }
    }
}
