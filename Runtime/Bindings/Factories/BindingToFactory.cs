using System;
using Uniject.Bindings;
using Uniject.InstanceGetters;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactory<TResult, TFactory> : Binding where TFactory : Factory<TResult>, new()
    {
        public InstanceGetter InstanceGetter { get; set; }
        public Type ResultContractType { get; set; }
        public Type ResultConcreteType { get; set; }

        public BindingToFactory(Container container, Type resultType, Type factoryType) : base(container, factoryType)
        {
            ResultContractType = resultType;
            ResultConcreteType = resultType;
            InstanceGetter = new InstanceGetterFromConstructor(container);
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