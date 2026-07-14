using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromConstructor : InstanceGetter
    {
        public InstanceGetterFromConstructor(Container container) : base(container) { }

        public override object GetInstance(Type concreteType, CreateOptions createOptions)
        {
            return _container.Instantiate(concreteType);
        }
    }
}