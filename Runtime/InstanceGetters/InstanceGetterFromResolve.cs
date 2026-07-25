using System;
using Uniject.Bindings;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromResolve : InstanceGetter
    {
        public InstanceGetterFromResolve(Container container, Type contractType, Type concreteType) : base(container)
        {
            if (contractType == concreteType)
                throw new ArgumentException($"Contract type and concrete type for {nameof(InstanceGetterFromResolve)} must be different types.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            return ResolveWithContext(concreteType, context);
        }
    }
}
