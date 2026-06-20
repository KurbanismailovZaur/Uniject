using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToType : Binding
    {
        public InstanceGetter InstanceGetter { get; set; }
        public bool IsNonLazy { get; set; }
        public bool IsEntryPoint { get; set; }
        public string ObjectName { get; set; }
        public Transform ParentTransform { get; set; }

        public BindingToType(Container container, Type contractType) : base(container, contractType)
        {
            InstanceGetter = new FromConstructorGetter(container);
        }

        protected virtual object CreateAndConfigureInstance()
        {
            var instance = InstanceGetter.GetInstance(ConcreteType);

            if (instance is GameObject gameObject)
            {
                if (ObjectName != null)
                    gameObject.name = ObjectName;

                if (ParentTransform != null)
                    gameObject.transform.SetParent(ParentTransform);
                else if (Container.ParentTransformForGameObjects != null)
                    gameObject.transform.SetParent(Container.ParentTransformForGameObjects);
            }
            else if (instance is Component component)
            {
                if (ObjectName != null)
                    component.gameObject.name = ObjectName;

                if (ParentTransform != null)
                    component.transform.SetParent(ParentTransform);
                else if (Container.ParentTransformForGameObjects != null)
                    component.transform.SetParent(Container.ParentTransformForGameObjects);
            }

            return instance;
        }

        public override object GetInstance()
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