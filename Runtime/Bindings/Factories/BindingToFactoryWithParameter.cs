using System;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

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
            InstanceGetter = new InstanceGetterWithParameterFromComponentInNewPrefab<TParam>(container, ParamType, ResultContractType);
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