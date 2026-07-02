using System;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterFromFactory<TResult, TFactory> : InstanceGetter 
        where TFactory : CustomFactory<TResult>, new()
    {
        private readonly TFactory _factory;

        public InstanceGetterFromFactory(Container container) : base(container)
        {
            _factory = new TFactory();
            _factory.Construct(_container);
            _factory.InitializeInternal();
        }

        public override object GetInstance(Type concreteType) => _factory.Create();
    }
}