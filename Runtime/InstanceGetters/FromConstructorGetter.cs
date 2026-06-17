using System;

namespace Uniject.InstanceGetters
{
    public class FromConstructorGetter : InstanceGetter
    {
        public FromConstructorGetter(Container container) : base(container) { }

        public override object GetInstance(Type concreteType) => _container.Instantiate(concreteType);
    }
}