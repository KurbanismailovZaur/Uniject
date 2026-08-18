using System;
using Uniject.Installers;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByInstaller : SubcontainerGetter
    {
        private readonly IInstaller _installer;

        internal override bool IsOwnedByParent => true;

        public SubcontainerGetterByInstaller(Container container, IInstaller installer) : base(container)
        {
            _installer = installer;
        }

        public override Container GetContainer()
        {
            var container = new Container(_container);

            try
            {
                _installer.Install(container);
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
