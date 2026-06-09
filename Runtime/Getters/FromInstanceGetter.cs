using System;

namespace Uniject.Getters
{
    public class FromInstanceGetter : InstanceGetter
    {
        private readonly object _instance;

        public FromInstanceGetter(Container container, object instance, Type concreteType) : base(container)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), $"Instance for {nameof(FromInstanceGetter)} can not be null.");

            if (!concreteType.IsAssignableFrom(instance.GetType()))
                throw new ArgumentNullException(nameof(instance), $"Instance must be assignable with type {concreteType}.");

            _instance = instance;
        }

        public override object GetInstance(Type concreteType) => _instance;
    }
}