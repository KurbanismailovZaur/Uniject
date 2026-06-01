using System;
using Uniject.Getters;

namespace Uniject.Bindings
{
    public class Binding
    {
        public Type ConcreteType { get; private set; }
        public InstanceGetter InstanceGetter { get; private set; }
        public Scope Scope { get; private set; }
        public object CachedInstance { get; private set; }
        public bool IsNonLazy { get; private set; }

        public Binding(Container container, Type concreteType) 
            : this(concreteType, new FromConstructorGetter(container), Scope.Transient, false) { }

        public Binding(Type concreteType, InstanceGetter instanceGetter, Scope scope, bool isNonLazy)
        {
            ConcreteType = concreteType;
            InstanceGetter = instanceGetter;
            Scope = scope;
            IsNonLazy = isNonLazy;
        }

        public void To(Type concreteType) => ConcreteType = concreteType;

        public void From(InstanceGetter instanceGetter) => InstanceGetter = instanceGetter;
        
        public void As(Scope scope) => Scope = scope;

        public void NonLazy() => IsNonLazy = true;

        public object GetObject()
        {
            if (Scope == Scope.Transient)
                return InstanceGetter.GetObject(ConcreteType);

            return CachedInstance ??= InstanceGetter.GetObject(ConcreteType);
        }
    }
}