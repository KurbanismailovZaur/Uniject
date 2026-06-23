using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByInstance : SubcontainerGetter
    {
        private Container _instance;

        public SubcontainerGetterByInstance(Container container, Container instance) : base(container)
        {
            _instance = instance;
        }

        public override Container GetContainer() => _instance;
    }
}
