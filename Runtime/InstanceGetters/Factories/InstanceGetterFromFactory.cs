using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterFromFactory<TResult, TFactory> : InstanceGetter 
        where TFactory : CustomFactory<TResult>, new()
    {
        private readonly TFactory _factory;

        public InstanceGetterFromFactory(Container container) : base(container)
        {
            _factory = new TFactory();
            _factory.Construct(Container);
            _factory.InitializeInternal();
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context) => _factory.Create();
    }
}
