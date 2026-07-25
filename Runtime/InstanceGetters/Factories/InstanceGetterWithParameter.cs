using System;

namespace Uniject.InstanceGetters.Factories
{
    public abstract class InstanceGetterWithParameter<TParam> : InstanceGetterBase
    {
        public InstanceGetterWithParameter(Container container): base(container) { }

        public abstract object GetInstance(
            Type concreteType,
            TParam origin,
            InjectContext context);
    }
}
