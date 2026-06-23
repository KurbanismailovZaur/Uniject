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
            // TODO: по умолчанию должен присваивать геттер через метод который не будет иметь биндингов.
            // TODO: надо еще вызывать Build у подконтейнера.

            // _subcontainerGetter = ?
        }

        public override object GetInstance(Type concreteType)
        {
            if (Scope == Scope.Transient)
            {
                var container = SubcontainerGetter.GetContainer();
                container.Build();
                return container.Resolve(concreteType);
            }

            if (CachedContainer != null)
                return CachedContainer.Resolve(concreteType);

            CachedContainer = SubcontainerGetter.GetContainer();
            CachedContainer.Build();
            return CachedContainer.Resolve(concreteType);
        }
    }
}