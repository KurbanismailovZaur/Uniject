using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public abstract class SubcontainerGetter
    {
        protected readonly Container _container;

        public SubcontainerGetter(Container container) => _container = container;

        public abstract Container GetContainer();
    }
}
