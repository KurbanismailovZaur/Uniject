using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToSubcontainerUnderTransformBuilder
    {
        protected readonly Container _container;
        protected readonly BindingToType _binding;
        protected readonly InstanceGetterFromSubContainerResolve _instanceGetter;

        public BindingToSubcontainerUnderTransformBuilder(Container container, BindingToType binding, InstanceGetterFromSubContainerResolve instanceGetter)
        {
            _container = container;
            _binding = binding;
            _instanceGetter = instanceGetter;
        }

        public BindingToTypeAsBuilder UnderTransform(Transform parent)
        {
            _instanceGetter.SubcontainerGetter.ContextUnderTransform = parent;
            return new BindingToTypeAsBuilder(_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsTransient()
        {
            _instanceGetter.Scope = Scope.Transient;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsCached()
        {
            _instanceGetter.Scope = Scope.Cached;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();
        
        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}