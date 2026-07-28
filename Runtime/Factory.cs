using System;
using Uniject.Bindings;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;

namespace Uniject
{
    public abstract class Factory
    {
        protected InstanceGetterBase _instanceGetter;
        protected Type _resultContractType;
        protected Type _resultConcreteType;
    }

    public class Factory<TResult> : Factory, IFactory<TResult>
    {
        internal void Construct(InstanceGetter instanceGetter, Type resultContractType, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultContractType = resultContractType;
            _resultConcreteType = resultConcreteType;
        }

        public TResult Create()
        {
            var context = InjectContext.CreateRoot(_instanceGetter.Container, _resultContractType);
            return (TResult)((InstanceGetter)_instanceGetter).GetInstance(
                _resultConcreteType,
                CreateOptions.Default,
                context);
        }
    }

    public class Factory<TParam, TResult> : Factory, IFactory<TParam, TResult>
    {
        internal void Construct( InstanceGetterWithParameter<TParam> instanceGetter, Type resultContractType, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultContractType = resultContractType;
            _resultConcreteType = resultConcreteType;
        }

        public TResult Create(TParam origin)
        {
            var context = InjectContext.CreateRoot(_instanceGetter.Container, _resultContractType);
            return (TResult)((InstanceGetterWithParameter<TParam>)_instanceGetter).GetInstance(
                _resultConcreteType,
                origin,
                context);
        }
    }
}
