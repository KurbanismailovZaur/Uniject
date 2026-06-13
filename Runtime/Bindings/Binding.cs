using System;
using Uniject.Getters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class Binding
    {
        public Container Container { get; set; }
        public Type ContractType { get; set; }
        public Type ConcreteType { get; set; }
        public InstanceGetter InstanceGetter { get; set; }
        public Scope Scope { get; set; }
        public object CachedInstance { get; private set; }
        public bool IsNonLazy { get; set; }
        public bool IsEntryPoint { get; set; }
        public string ObjectName { get; set; }
        public Transform ParentTransform { get; set; }

        public Binding(Container container, Type contractType) 
        {
            Container = container;
            ContractType = contractType;
            ConcreteType = contractType;
            InstanceGetter = new FromConstructorGetter(container);
            Scope = Scope.Transient;
        }

        private object CreateAndConfigureInstance()
        {
            var instance = InstanceGetter.GetInstance(ConcreteType);

            if(instance is GameObject gameObject)
            {
                if (ObjectName != null)
                    gameObject.name = ObjectName;

                if (ParentTransform != null)
                    gameObject.transform.SetParent(ParentTransform);
            }
            else if (instance is Component component)
            {
                if (ObjectName != null)
                    component.gameObject.name = ObjectName;

                if (ParentTransform != null)
                    component.transform.SetParent(ParentTransform);
            }

            return instance;
        }

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

                return CreateAndConfigureInstance();
            }

            return CachedInstance ??= CreateAndConfigureInstance();
        }

        internal void PrepareNonLazyInstance() => CachedInstance ??= CreateAndConfigureInstance();
    }
}