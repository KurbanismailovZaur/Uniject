using System;
using Uniject.Bindings;
using Uniject.SubcontainerGetters;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromSubContainerResolve : InstanceGetter
    {
        public SubcontainerGetter SubcontainerGetter { get; set; }
        public Scope Scope { get; set; }
        public Container CachedInstance { get; protected set; }

        public InstanceGetterFromSubContainerResolve(Container container) : base(container)
        {
            // TODO: по умолчанию должен присваивать геттер через метод который не будет иметь биндингов.
            // TODO: надо еще вызывать Build у подконтейнера.

            // _subcontainerGetter = ?
        }

        public override object GetInstance(Type concreteType)
        {
            if (Scope == Scope.Transient)
                return SubcontainerGetter.GetContainer().Resolve(concreteType);

            return (CachedInstance ??= SubcontainerGetter.GetContainer()).Resolve(concreteType);
        }
    }
}