using System;

namespace Uniject.InstanceGetters
{
    public abstract class InstanceGetterBase
    {
        internal Container Container { get; private set; }

        public InstanceGetterBase(Container container) => Container = container;

        protected object ResolveWithContext(Type contractType, InjectContext context)
        {
            return Container.Resolve(contractType, context);
        }
    }
}
