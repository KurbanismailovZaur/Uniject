using System;
using Uniject.InstanceGetters;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToType : Binding
    {
        public InstanceGetter InstanceGetter { get; set; }
        public bool IsNonLazy { get; set; }
        public bool IsEntryPoint { get; set; }
        public bool ShouldDisposeWithContainer { get; private set; }
        public string ObjectName { get; set; }
        public Transform UnderTransform { get; set; }
        public bool IsNonLazyCreated { get; private set; }

        public BindingToType(Container container, Type contractType) : base(container, contractType)
        {
            InstanceGetter = new InstanceGetterFromConstructor(container);
        }

        protected virtual object CreateAndConfigureInstance(InjectContext injectContext)
        {
            if (ShouldDisposeWithContainer && Scope != Scope.Cached)
                throw new InvalidOperationException("A binding disposed with the container must remain cached.");

            IsNonLazyCreated = true;

            var (context, parentTransform) = Container.GetInfoAboutNearestParentForGameObjects();
            var createOptions = new CreateOptions(ObjectName, UnderTransform, parentTransform, context);
            var instance = InstanceGetter.GetInstance(ConcreteType, createOptions, injectContext);

            if (ShouldDisposeWithContainer)
                Container.RegisterDisposable(instance, ContractType);

            return instance;
        }

        protected internal override object GetInstance(InjectContext context)
        {
            if (Scope == Scope.Transient)
            {
                if (CachedInstance != null)
                {
                    var cachedInstance = CachedInstance;
                    CachedInstance = null;
                    return cachedInstance;
                }

                return CreateAndConfigureInstance(context);
            }

            return CachedInstance ??= CreateAndConfigureInstance(context);
        }

        internal void PrepareNonLazyInstance()
        {
            if (!IsNonLazyCreated)
                CachedInstance ??= CreateAndConfigureInstance(InjectContext.CreateRoot(Container, ContractType));
        }

        internal void ConfigureScope(Scope scope)
        {
            EnsureCanConfigure();
            Scope = scope;
        }

        internal void ConfigureConcreteType(Type concreteType)
        {
            EnsureCanConfigure();
            ConcreteType = concreteType;
        }

        internal void ConfigureInstanceGetter(InstanceGetter instanceGetter)
        {
            EnsureCanConfigure();
            InstanceGetter = instanceGetter;
        }

        internal void ConfigureObjectName(string objectName)
        {
            EnsureCanConfigure();
            ObjectName = objectName;
        }

        internal void ConfigureUnderTransform(Transform underTransform)
        {
            EnsureCanConfigure();
            UnderTransform = underTransform;
        }

        internal void ConfigureNonLazy()
        {
            EnsureCanConfigure();
            IsNonLazy = true;
        }

        internal void ConfigureAsEntryPoint()
        {
            EnsureCanConfigure();

            if (!typeof(IEntryPoint).IsAssignableFrom(ConcreteType))
                throw new InvalidOperationException($"Type {ConcreteType} is not assignable from {typeof(IEntryPoint)}");

            Scope = Scope.Cached;
            IsEntryPoint = true;
        }

        internal void ConfigureDisposeWithContainer()
        {
            Container.ThrowIfDisposed();

            if (Scope != Scope.Cached)
                throw new InvalidOperationException("Only cached bindings can be disposed with the container.");

            if (CachedInstance != null)
                Container.RegisterDisposable(CachedInstance, ContractType);
            else if (InstanceGetter is InstanceGetterFromInstance instanceGetter)
                Container.RegisterDisposable(instanceGetter.Instance, ContractType);

            ShouldDisposeWithContainer = true;
        }

        internal void EnsureCanConfigure()
        {
            Container.ThrowIfDisposed();

            if (ShouldDisposeWithContainer)
                throw new InvalidOperationException("The binding configuration is finalized by DisposeWithContainer().");
        }
    }
}
