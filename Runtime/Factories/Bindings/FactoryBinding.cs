using System;
using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBinding<TResult, TFactory> : BindingBase where TFactory : Factory, new()
    {
        public Type ResultContractType { get; set; }
        public Type ResultConcreteType { get; set; }

        public FactoryBinding(Container container, Type resultType, Type factoryType) : base(container, factoryType)
        {
            ResultContractType = resultType;
            ResultConcreteType = resultType;
        }

        private object CreateFactory()
        {
            var factory = new TFactory();
            factory.Construct(InstanceGetter, ResultConcreteType);
            return factory;
        }

        public override object GetInstance()
        {
            if (Scope == Scope.Transient)
                return CreateFactory();

            return CachedInstance ??= CreateFactory();
        }
    }
}