using System;

namespace Uniject.Getters
{
    public class FromInstanceGetter : InstanceGetter
    {
        private object _instance;

        public FromInstanceGetter(Container container, object instance) : base(container)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance), 
                $"Instance for FromInstance getter can not be null.");
        }

        public override object GetObject(Type concreteType) => _instance;
    }
}