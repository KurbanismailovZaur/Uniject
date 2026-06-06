using System;

namespace Uniject.Getters
{
    public class FromInstanceGetter<TConcrete> : InstanceGetter
    {
        private TConcrete _instance;

        public FromInstanceGetter(Container container, TConcrete instance) : base(container)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance), 
                $"Instance for {nameof(FromInstanceGetter<TConcrete>)} getter can not be null.");
        }

        public override object GetInstance(Type concreteType) => _instance;
    }
}