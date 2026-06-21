using System;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterWithParameterFromFactory<TParam, TResult, TFactory> : InstanceGetterWithParameter<TParam> 
        where TFactory : IFactory<TParam, TResult>, new()
    {
        private readonly TFactory _factory;

        public InstanceGetterWithParameterFromFactory(Container container) : base(container)
        {
            _factory = new TFactory();
            _container.AddToInjectionQueue(_factory);
        }

        public override object GetInstance(Type concreteType, TParam origin) => _factory.Create(origin);
    }
}