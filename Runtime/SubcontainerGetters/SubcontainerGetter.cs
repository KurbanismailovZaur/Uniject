using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.SubcontainerGetters
{
    public abstract class SubcontainerGetter
    {
        protected readonly Container _container;

        internal virtual bool IsOwnedByParent => false;
        
        public string ContextGameObjectName { get; set; }
        
        public Transform ContextUnderTransform { get; set; }

        public SubcontainerGetter(Container container) => _container = container;

        public abstract Container GetContainer();
    }
}
