using System;
using Uniject.Bindings;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;

namespace Uniject
{
    public abstract class Factory
    {
        protected InstanceGetterBase _instanceGetter;
        protected Type _resultConcreteType;
    }

    public class Factory<TResult> : Factory, IFactory<TResult>
    {
        internal void Construct(InstanceGetter instanceGetter, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        public TResult Create() => (TResult)((InstanceGetter)_instanceGetter).GetInstance(_resultConcreteType, CreateOptions.Default);
    }

    public class Factory<TParam, TResult> : Factory, IFactory<TParam, TResult>
    {
        internal void Construct(InstanceGetterWithParameter<TParam> instanceGetter, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        public TResult Create(TParam origin) => (TResult)((InstanceGetterWithParameter<TParam>)_instanceGetter).GetInstance(_resultConcreteType, origin);
    }
}