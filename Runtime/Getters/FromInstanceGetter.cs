using System;

namespace Uniject.Getters
{
    public class FromInstanceGetter<TConcrete> : InstanceGetter
    {
        private readonly TConcrete _instance;

        public FromInstanceGetter(Container container, TConcrete instance) : base(container)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), 
                    $"Instance for {nameof(FromInstanceGetter<TConcrete>)} can not be null.");

            _instance = instance;
        }

        public override object GetInstance(Type concreteType) => _instance;
    }
}