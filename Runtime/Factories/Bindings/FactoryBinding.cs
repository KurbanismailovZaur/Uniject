using System;
using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBinding<TResultContract> : BindingBase
    {
        public Type ResultContractType { get; set; }
        public Type ResultConcreteType { get; set; }

        public FactoryBinding(Container container, Type resultType, Type factoryType) : base(container, factoryType)
        {
            ResultContractType = resultType;
            ResultConcreteType = resultType;
        }

        private object CreateFactory() => new Factory<TResultContract>(InstanceGetter, ResultConcreteType);

        public override object GetInstance()
        {
            if (Scope == Scope.Transient)
                return CreateFactory();

            return CachedInstance ??= CreateFactory();
        }
    }
}