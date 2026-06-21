using System;
using UnityEngine;

namespace Uniject.Reflection
{
    public static class TypeValidator
    {
        public static bool TypeCanBeAddedAsComponent(Type type) => !type.IsAbstract && TypeIsComponent(type);

        public static bool TypeIsComponent(Type type) => typeof(Component).IsAssignableFrom(type);

        public static bool TypeIsInterfaceOrComponent(Type type) => type.IsInterface || TypeIsComponent(type);
    }
}
