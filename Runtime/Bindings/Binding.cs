using System;
using Uniject.Getters;

namespace Uniject.Bindings
{
    public class Binding
    {
        public Container Container { get; private set; }
        public Type ContractType { get; private set; }
        public Type ConcreteType { get; private set; }
        public InstanceGetter InstanceGetter { get; private set; }
        public Scope Scope { get; private set; }
        private object CachedInstance { get; set; }

        public Binding(Container container, Type contractType) 
        {
            Container = container;
            ContractType = contractType;
            ConcreteType = contractType;
            InstanceGetter = new FromConstructorGetter(container);
            Scope = Scope.Transient;
        }

        public void To(Type concreteType) => ConcreteType = concreteType;

        public void From(InstanceGetter instanceGetter) => InstanceGetter = instanceGetter;
        
        public void As(Scope scope) => Scope = scope;

        public void NonLazy() => Container.MarkBindingNonLazy(this);

        public object GetInstance()
        {
            if (Scope == Scope.Transient)
            {
                if (CachedInstance != null)
                {
                    var cachedInstance = CachedInstance;
                    CachedInstance = null;
                    return cachedInstance;    
                }

                return InstanceGetter.GetObject(ConcreteType);
            }

            return CachedInstance ??= InstanceGetter.GetObject(ConcreteType);
        }

        internal void PrepareNonLazyInstance() => CachedInstance ??= InstanceGetter.GetObject(ConcreteType);
    }
}