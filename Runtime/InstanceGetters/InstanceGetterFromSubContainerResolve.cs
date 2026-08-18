using System;
using Uniject.Bindings;
using Uniject.SubcontainerGetters;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromSubContainerResolve : InstanceGetter
    {
        public SubcontainerGetter SubcontainerGetter { get; set; }
        public Scope Scope { get; set; }
        public Container CachedContainer { get; protected set; }

        public InstanceGetterFromSubContainerResolve(Container container) : base(container)
        {
            SubcontainerGetter = new SubcontainerGetterByInstance(container, new Container());
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            if (Scope == Scope.Transient)
            {
                var container = SubcontainerGetter.GetContainer();
                return BuildAndResolve(container, concreteType, context, false);
            }

            if (CachedContainer != null)
                return CachedContainer.Resolve(concreteType, context);

            var cachedContainer = SubcontainerGetter.GetContainer();
            return BuildAndResolve(cachedContainer, concreteType, context, true);
        }

        private object BuildAndResolve(
            Container container,
            Type concreteType,
            InjectContext context,
            bool cacheContainer)
        {
            var isOwnedByParent = SubcontainerGetter.IsOwnedByParent;
            var isRegistered = false;

            try
            {
                if (isOwnedByParent)
                {
                    Container.RegisterOwnedChildContainer(container);
                    isRegistered = true;
                }

                container.Build();
                var instance = container.Resolve(concreteType, context);

                if (cacheContainer)
                    CachedContainer = container;

                return instance;
            }
            catch (Exception resolveException)
            {
                if (isOwnedByParent)
                {
                    Exception cleanupException = null;

                    if (isRegistered)
                    {
                        try
                        {
                            Container.UnregisterOwnedChildContainer(container);
                        }
                        catch (Exception unregisterException)
                        {
                            cleanupException = unregisterException;
                        }
                    }

                    try
                    {
                        container.Dispose();
                    }
                    catch (Exception disposeException)
                    {
                        cleanupException = cleanupException == null
                            ? disposeException
                            : new AggregateException(cleanupException, disposeException);
                    }

                    if (cleanupException != null)
                        throw new AggregateException(resolveException, cleanupException).Flatten();
                }

                throw;
            }
        }
    }
}
