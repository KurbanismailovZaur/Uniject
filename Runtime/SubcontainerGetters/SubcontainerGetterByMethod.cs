using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByMethod : SubcontainerGetter
    {
        private readonly Action<Container> _installMethod;

        public SubcontainerGetterByMethod(Container container, Action<Container> installMethod) : base(container)
        {
            _installMethod = installMethod;
        }

        public override Container GetContainer()
        {
            var container = new Container(_container);
            _installMethod?.Invoke(container);
            return container;
        }
    }
}
