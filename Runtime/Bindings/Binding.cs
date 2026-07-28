using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public abstract class Binding
    {
        public Container Container { get; set; }
        public Type ContractType { get; set; }
        public Type ConcreteType { get; set; }
        public Scope Scope { get; set; }
        public object CachedInstance { get; protected set; }

        public Binding(Container container, Type contractType)
        {
            Container = container;
            ContractType = contractType;
            ConcreteType = contractType;
            Scope = Scope.Transient;
        }

        public object GetInstance() => GetInstance(InjectContext.CreateRoot(Container, ContractType));

        protected internal abstract object GetInstance(InjectContext context);
    }
}
