using System;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterFromFactory<TResult, TFactory> : InstanceGetter where TFactory : IFactory<TResult>, new()
    {
        private readonly TFactory _factory;

        public InstanceGetterFromFactory(Container container) : base(container)
        {
            _factory = new TFactory();
            _container.AddToInjectionQueue(_factory);
        }

        public override object GetInstance(Type concreteType) => _factory.Create();
    }
}