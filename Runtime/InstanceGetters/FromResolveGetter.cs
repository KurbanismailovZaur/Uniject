using System;

namespace Uniject.InstanceGetters
{
    public class FromResolveGetter : InstanceGetter
    {
        public FromResolveGetter(Container container, Type contractType, Type concreteType) : base(container)
        {
            if (contractType == concreteType)
                throw new ArgumentException($"Contract type and concrete type for {nameof(FromResolveGetter)} must be different types.");
        }

        public override object GetInstance(Type concreteType) => _container.Resolve(concreteType);
    }
}