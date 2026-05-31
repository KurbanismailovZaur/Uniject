using System;

namespace Uniject.Getters
{
    public class FromInstanceGetter : InstanceGetter
    {
        private object _instance;

        public FromInstanceGetter(Container container, object instance) : base(container)
        {
            _instance = instance;
        }

        public override object GetObject(Type concreteType)
        {
            return _instance;
        }
    }
}