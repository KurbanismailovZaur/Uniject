using System;

namespace Uniject.InstanceGetters
{
    public abstract class InstanceGetter : InstanceGetterBase
    {
        public InstanceGetter(Container container) : base(container) { }

        public abstract object GetInstance(Type concreteType);
    }
}