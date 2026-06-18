using System;
using Uniject.Attributes;
using Uniject.InstanceGetters;

namespace Uniject.Factories
{
    public class Factory<TResult> 
    {
        private InstanceGetter _instanceGetter;
        private readonly Type _resultConcreteType;

        public Factory(InstanceGetter instanceGetter, Type resultConcreteType)
        {
            _instanceGetter = instanceGetter;
            _resultConcreteType = resultConcreteType;
        }

        public TResult Create()
        {
            return (TResult)_instanceGetter.GetInstance(_resultConcreteType);
        }
    }
}