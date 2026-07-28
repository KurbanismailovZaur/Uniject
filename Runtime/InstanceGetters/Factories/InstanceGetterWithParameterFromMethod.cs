using System;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterWithParameterFromMethod<TParam, TResult> : InstanceGetterWithParameter<TParam>
    {
        private readonly Func<TParam, InjectContext, TResult> _method;

        public InstanceGetterWithParameterFromMethod(
            Container container,
            Func<TParam, InjectContext, TResult> method) : base(container)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method),
                    $"Method for {nameof(InstanceGetterWithParameterFromMethod<TParam, TResult>)} can not be null.");

            _method = method;
        }

        public override object GetInstance(
            Type concreteType,
            TParam origin,
            InjectContext context)
        {
            var instance = _method(origin, context);

            if (instance is null)
                throw new InvalidOperationException(
                    $"Method for {nameof(InstanceGetterWithParameterFromMethod<TParam, TResult>)} returned null.");

            if (!concreteType.IsInstanceOfType(instance))
                throw new InvalidOperationException(
                    $"Method for {nameof(InstanceGetterWithParameterFromMethod<TParam, TResult>)} returned instance of type " +
                    $"{instance.GetType()}, which is not assignable to {concreteType}.");

            return instance;
        }
    }
}
