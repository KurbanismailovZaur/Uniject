using System;
using Uniject.Bindings;
using Uniject.Contexts;
using Uniject.Reflection;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromNewComponentOnRoot : InstanceGetter
    {
        public InstanceGetterFromNewComponentOnRoot(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeCanBeAddedAsComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromNewComponentOnRoot)} " +
                    "must be a non-abstract Component.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            foreach (var currentContainer in context.Container.GetSelfAndParents())
            {
                var currentContext = currentContainer.Context;

                if (ReferenceEquals(currentContext, null))
                    continue;

                if (currentContext == null)
                    throw new InvalidOperationException(
                        $"{nameof(InstanceGetterFromNewComponentOnRoot)} encountered a destroyed Context " +
                        $"while creating component of type {concreteType}.");

                if (currentContext is not GameObjectContext && currentContext is not SceneContext)
                    throw new InvalidOperationException(
                        $"{nameof(InstanceGetterFromNewComponentOnRoot)} does not support Context type " +
                        $"{currentContext.GetType()}. Only GameObjectContext and SceneContext are supported.");

                return context.Container.AddComponent(currentContext.gameObject, concreteType);
            }

            throw new InvalidOperationException(
                $"{nameof(InstanceGetterFromNewComponentOnRoot)} requires a live GameObjectContext " +
                $"or SceneContext in the binding owner's container hierarchy to create component " +
                $"of type {concreteType}.");
        }
    }
}
