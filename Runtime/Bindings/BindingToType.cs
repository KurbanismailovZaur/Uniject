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

            var createOptions = new CreateOptions(ObjectName, UnderTransform, 
                Container.ParentTransformForGameObjects, Container.GetNearestContext().transform);

            return InstanceGetter.GetInstance(ConcreteType, createOptions);
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