using System;

namespace Uniject
{
    public abstract class FromInstanceGetter
    {
        protected readonly Container _container;

        public FromInstanceGetter(Container container) => _container = container;

        public abstract object GetObject(Type concreteType);
    }
}