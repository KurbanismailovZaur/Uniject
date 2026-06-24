using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByInstance : SubcontainerGetter
    {
        private readonly Container _instance;

        public SubcontainerGetterByInstance(Container container, Container instance) : base(container)
        {
            _instance = instance;
            _instance.SetParentContainer(_container);
        }

        public override Container GetContainer() => _instance;
    }
}
