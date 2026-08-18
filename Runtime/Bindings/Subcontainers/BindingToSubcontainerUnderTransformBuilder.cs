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

        public BindingToSubcontainerAsBuilder UnderTransform(Transform parent)
        {
            _binding.EnsureCanConfigure();
            _instanceGetter.SubcontainerGetter.ContextUnderTransform = parent;
            return new BindingToSubcontainerAsBuilder(_container, _binding, _instanceGetter);
        }

        public BindingToTypeNonLazyBuilder AsTransient() => UnderTransform(null).AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => UnderTransform(null).AsCached();

        public BindingToTypeAsEntryPointBuilder NonLazy()=> AsCached().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
