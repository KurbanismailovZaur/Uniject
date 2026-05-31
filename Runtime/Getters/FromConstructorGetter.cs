using System;

namespace Uniject.Getters
{
    public class FromConstructorGetter : InstanceGetter
    {
        public FromConstructorGetter(Container container) : base(container) { }

        public override object GetObject(Type concreteType) => _container.Instantiate(concreteType);
    }
}