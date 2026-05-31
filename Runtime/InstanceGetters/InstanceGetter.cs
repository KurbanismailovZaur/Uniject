using System;

namespace Uniject
{
    public class InstanceGetter
    {
        private Type _concreteType;
        private FromInstanceGetter _concreteInstanceGetter;
        private Scope _scope;
        private object _cachedInstance;
        private bool _isNonLazy;

        public InstanceGetter(Container container, Type concreteType) 
            : this(concreteType, new FromConstructorInstanceGetter(container), Scope.Transient, false) { }

        public InstanceGetter(Type concreteType, FromInstanceGetter getterFrom, Scope scope, bool isNonLazy)
        {
            _concreteType = concreteType;
            _concreteInstanceGetter = getterFrom;
            _scope = scope;
            _isNonLazy = isNonLazy;
        }

        public object GetObject()
        {
            if (_scope == Scope.Transient)
                return _concreteInstanceGetter.GetObject(_concreteType);

            return _cachedInstance ??= _concreteInstanceGetter.GetObject(_concreteType);
        }
    }
}