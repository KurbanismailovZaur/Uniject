using System;
using System.Collections.Generic;
using System.Reflection;
using Uniject.Attributes;
using UnityEngine;

namespace Uniject.Reflection
{
    public static class TypeValidator
    {
        public static void ThrowIfCantBeInstantiatedFromConstructor(Type concreteType)
        {
            if (concreteType.IsInterface)
                throw new ArgumentException($"Type {concreteType} can not be instantiated because it is an interface.", 
                    nameof(concreteType));

            if (concreteType.IsAbstract)
                throw new ArgumentException($"Type {concreteType} can not be instantiated because it is abstract.", 
                    nameof(concreteType));

            if (typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} can not be instantiated from constructor because it is a Unity Component.",
                    nameof(concreteType));
        }

        public static bool TypeIsNotInterfaceOrComponent(Type type)
        {
            return !type.IsInterface && !typeof(Component).IsAssignableFrom(type);
        }
    }
}
