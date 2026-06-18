using System;
using Uniject.Attributes;
using Uniject.InstanceGetters;

namespace Uniject.Factories
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

    public class Factory<TResult> : Factory
    {
        public TResult Create()
        {
            return (TResult)_instanceGetter.GetInstance(_resultConcreteType);
        }
    }
}