using System;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterWithParameterFromMethod<TParam, TResult> : InstanceGetterWithParameter<TParam>
    {
        private readonly Func<Container, TParam, TResult> _method;

        public InstanceGetterWithParameterFromMethod(
            Container container,
            Func<Container, TParam, TResult> method) : base(container)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method),
                    $"Method for {nameof(InstanceGetterWithParameterFromMethod<TParam, TResult>)} can not be null.");

            _method = method;
        }

        public override object GetInstance(Type concreteType, TParam origin)
        {
            var instance = _method(_container, origin);

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
