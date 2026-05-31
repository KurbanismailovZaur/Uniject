using System;

namespace Uniject
{
    public class FromConstructorInstanceGetter : FromInstanceGetter
    {
        public FromConstructorInstanceGetter(Container container) : base(container) { }

        public override object GetObject(Type concreteType)
        {
            return _container.Instantiate<object>(concreteType);
        }
    }
}