using System;
using Uniject.Getters;

namespace Uniject.Bindings
{
    public class Binding
    {
        private Type _concreteType;
        private InstanceGetter _instanceGetter;
        private Scope _scope;
        private object _cachedInstance;
        private bool _isNonLazy;

        public Binding(Container container, Type concreteType) 
            : this(concreteType, new FromConstructorGetter(container), Scope.Transient, false) { }

        public Binding(Type concreteType, InstanceGetter instanceGetter, Scope scope, bool isNonLazy)
        {
            _concreteType = concreteType;
            _instanceGetter = instanceGetter;
            _scope = scope;
            _isNonLazy = isNonLazy;
        }

        public void To(Type concreteType) => _concreteType = concreteType;

        public void From(InstanceGetter instanceGetter) => _instanceGetter = instanceGetter;
        
        public void As(Scope scope) => _scope = scope;

        public void NonLazy() => _isNonLazy = true;

        public object GetObject()
        {
            if (_scope == Scope.Transient)
                return _instanceGetter.GetObject(_concreteType);

            return _cachedInstance ??= _instanceGetter.GetObject(_concreteType);
        }
    }
}