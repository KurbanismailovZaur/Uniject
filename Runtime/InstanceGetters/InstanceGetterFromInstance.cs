using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromInstance : InstanceGetter
    {
        private readonly object _instance;

        public InstanceGetterFromInstance(Container container, object instance, Type concreteType) : base(container)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance), $"Instance for {nameof(InstanceGetterFromInstance)} can not be null.");

            if (!concreteType.IsAssignableFrom(instance.GetType()))
                throw new ArgumentNullException(nameof(instance), $"Instance must be assignable with type {concreteType}.");

            _instance = instance;
        }

        public override object GetInstance(Type concreteType, CreateOptions createOptions) => _instance;
    }
}