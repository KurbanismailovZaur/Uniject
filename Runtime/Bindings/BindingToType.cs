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
        public Transform UnderTransform { get; set; }
        public bool IsNonLazyCreated { get; private set; }

        public BindingToType(Container container, Type contractType) : base(container, contractType)
        {
            InstanceGetter = new InstanceGetterFromConstructor(container);
        }

        protected virtual object CreateAndConfigureInstance()
        {
            IsNonLazyCreated = true;

            var objectCreateOptions = new ObjectCreateOptions(ObjectName, UnderTransform, 
                Container.ParentTransformForGameObjects, Container.GetNearestContext().transform);
                
            var instance = InstanceGetter.GetInstance(ConcreteType);

            if (instance is GameObject gameObject)
            {
                if (ObjectName != null)
                    gameObject.name = ObjectName;

                if (UnderTransform != null)
                    gameObject.transform.SetParent(UnderTransform);
                else if (Container.ParentTransformForGameObjects != null)
                    gameObject.transform.SetParent(Container.ParentTransformForGameObjects);
            }
            else if (instance is Component component)
            {
                if (ObjectName != null)
                    component.gameObject.name = ObjectName;

                if (UnderTransform != null)
                    component.transform.SetParent(UnderTransform);
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

        internal void PrepareNonLazyInstance()
        {
            if (!IsNonLazyCreated)
                CachedInstance ??= CreateAndConfigureInstance();
        }
    }
}