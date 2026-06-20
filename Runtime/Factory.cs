using System;
using Uniject.InstanceGetters;

namespace Uniject
{
    public abstract class Factory
    {
        protected InstanceGetter _instanceGetter;
        protected Type _resultConcreteType;

        internal void Construct(InstanceGetter instanceGetter, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }
    }

    public class Factory<TResult> : Factory, IFactory<TResult>
    {
        public TResult Create() => (TResult)_instanceGetter.GetInstance(_resultConcreteType);
    }

    public class Factory<TParam, TResult> : Factory, IFactory<TParam, TResult>
    {
        public TResult Create(TParam origin) => (TResult)_instanceGetter.GetInstance(_resultConcreteType);
    }
}