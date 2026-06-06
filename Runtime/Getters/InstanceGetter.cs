using System;

namespace Uniject.Getters
{
    public abstract class InstanceGetter
    {
        protected readonly Container _container;

        public InstanceGetter(Container container) => _container = container;

        public abstract object GetInstance(Type concreteType);
    }
}