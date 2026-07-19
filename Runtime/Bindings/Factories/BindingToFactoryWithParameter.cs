using System;
using Uniject.InstanceGetters.Factories;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameter<TParam, TResult, TFactory> : Binding where TFactory : Factory<TParam, TResult>, new()
    {
        public InstanceGetterWithParameter<TParam> InstanceGetter { get; set; }
        public Type ParamType { get; set; }
        public Type ResultContractType { get; set; }
        public Type ResultConcreteType { get; set; }

        public BindingToFactoryWithParameter(Container container, Type paramType, Type resultType, Type factoryType) : base(container, factoryType)
        {
            ParamType = paramType;
            ResultContractType = resultType;
            ResultConcreteType = resultType;
        }

        private object CreateFactory()
        {
            if (InstanceGetter == null)
                throw new InvalidOperationException($"Source for parameterized factory {typeof(TFactory)} is not " + 
                    "configured. Use FromMethod(), FromComponentInNewPrefab(), FromNewComponentOnNewPrefab(), " +
                    "or FromFactory<TCustomFactory>().");

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
