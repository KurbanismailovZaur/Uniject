using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromMethod<TResult> : InstanceGetter
    {
        private readonly Func<InjectContext, TResult> _method;

        public InstanceGetterFromMethod(
            Container container,
            Func<InjectContext, TResult> method) : base(container)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method),
                    $"Method for {nameof(InstanceGetterFromMethod<TResult>)} can not be null.");

            _method = method;
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var instance = _method(context);

            if (instance is null)
                throw new InvalidOperationException(
                    $"Method for {nameof(InstanceGetterFromMethod<TResult>)} returned null.");

            if (!concreteType.IsInstanceOfType(instance))
                throw new InvalidOperationException(
                    $"Method for {nameof(InstanceGetterFromMethod<TResult>)} returned instance of type " +
                    $"{instance.GetType()}, which is not assignable to {concreteType}.");

            return instance;
        }
    }
}
