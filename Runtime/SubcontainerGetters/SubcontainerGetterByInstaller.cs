using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByInstaller : SubcontainerGetter
    {
        private readonly IInstaller _installer;

        public SubcontainerGetterByInstaller(Container container, IInstaller installer) : base(container)
        {
            _installer = installer;
        }

        public override Container GetContainer()
        {
            var container = new Container(_container);
            _installer.Install(container);
            return container;
        }
    }
}
