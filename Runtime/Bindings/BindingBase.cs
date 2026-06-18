using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public abstract class BindingBase
    {
        public Container Container { get; set; }
        public Type ContractType { get; set; }
        public Type ConcreteType { get; set; }
        public InstanceGetter InstanceGetter { get; set; }
        public Scope Scope { get; set; }
        public object CachedInstance { get; protected set; }

        public BindingBase(Container container, Type contractType)
        {
            Container = container;
            ContractType = contractType;
            ConcreteType = contractType;
            InstanceGetter = new FromConstructorGetter(container);
            Scope = Scope.Transient;
        }

        public abstract object GetInstance();
    }
}