using System;

namespace Uniject.InstanceGetters
{
    public abstract class InstanceGetterBase
    {
        protected readonly Container _container;

        public InstanceGetterBase(Container container) => _container = container;

        protected object ResolveWithContext(Type contractType, InjectContext context)
        {
            return _container.Resolve(contractType, context);
        }
    }
}
