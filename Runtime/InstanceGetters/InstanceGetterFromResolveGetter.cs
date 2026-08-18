using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromResolveGetter<TResolve, TResult> : InstanceGetter
    {
        private readonly Func<TResolve, TResult> _getter;

        public InstanceGetterFromResolveGetter(
            Container container,
            Func<TResolve, TResult> getter) : base(container)
        {
            if (getter == null)
                throw new ArgumentNullException(nameof(getter),
                    $"Getter for {nameof(InstanceGetterFromResolveGetter<TResolve, TResult>)} can not be null.");

            if (typeof(TResolve) == typeof(TResult))
                throw new ArgumentException(
                    $"Resolved type and result type for {nameof(InstanceGetterFromResolveGetter<TResolve, TResult>)} must be different types.");

            _getter = getter;
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var resolvedInstance = (TResolve)ResolveWithContext(typeof(TResolve), context);
            var instance = _getter(resolvedInstance);

            if (instance is null)
                throw new InvalidOperationException(
                    $"Getter for {nameof(InstanceGetterFromResolveGetter<TResolve, TResult>)} returned null.");

            if (!concreteType.IsInstanceOfType(instance))
                throw new InvalidOperationException(
                    $"Getter for {nameof(InstanceGetterFromResolveGetter<TResolve, TResult>)} returned instance of type " +
                    $"{instance.GetType()}, which is not assignable to {concreteType}.");

            return instance;
        }
    }
}
