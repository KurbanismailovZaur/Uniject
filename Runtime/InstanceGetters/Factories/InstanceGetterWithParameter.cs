using System;

namespace Uniject.InstanceGetters
{
    public abstract class InstanceGetterWithParameter<TParam>
    {
        protected readonly Container _container;

        public InstanceGetterWithParameter(Container container) => _container = container;

        public abstract object GetInstance(Type concreteType, TParam origin);
    }
}