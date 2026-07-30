using System;
using System.Collections.Generic;
using Uniject.Bindings;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromComponentInChildren : InstanceGetter
    {
        public InstanceGetterFromComponentInChildren(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromComponentInChildren)} " +
                    "must be a Component or an interface.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            if (context.ConsumerInstance is not MonoBehaviour consumer || consumer == null)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromComponentInChildren)} can only be used during method injection " +
                    "into a live MonoBehaviour.");

            var pendingTransforms = new Stack<Transform>();
            pendingTransforms.Push(consumer.transform);

            while (pendingTransforms.Count > 0)
            {
                var currentTransform = pendingTransforms.Pop();

                if (currentTransform == null)
                    continue;

                var component = currentTransform.gameObject.GetComponent(concreteType);

                if (component != null)
                    return component;

                for (var i = currentTransform.childCount - 1; i >= 0; i--)
                    pendingTransforms.Push(currentTransform.GetChild(i));
            }

            throw new InvalidOperationException(
                $"{nameof(InstanceGetterFromComponentInChildren)} could not find a component assignable to type " +
                $"{concreteType} on GameObject '{consumer.gameObject.name}' or any of its children " +
                $"for consumer {consumer.GetType()}.");
        }
    }
}
