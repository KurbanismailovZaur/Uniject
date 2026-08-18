using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByMethod : SubcontainerGetter
    {
        private readonly Action<Container> _installMethod;

        internal override bool IsOwnedByParent => true;

        public SubcontainerGetterByMethod(Container container, Action<Container> installMethod) : base(container)
        {
            _installMethod = installMethod;
        }

        public override Container GetContainer()
        {
            var container = new Container(_container);

            try
            {
                _installMethod?.Invoke(container);
                return container;
            }
            catch (Exception installException)
            {
                try
                {
                    container.Dispose();
                }
                catch (Exception disposeException)
                {
                    throw new AggregateException(installException, disposeException).Flatten();
                }

                throw;
            }
        }
    }
}
