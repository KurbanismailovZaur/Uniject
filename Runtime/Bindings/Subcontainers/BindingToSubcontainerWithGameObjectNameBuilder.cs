using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToSubcontainerWithGameObjectNameBuilder
    {
        protected readonly Container _container;
        protected readonly BindingToType _binding;
        protected readonly InstanceGetterFromSubContainerResolve _instanceGetter;

        public BindingToSubcontainerWithGameObjectNameBuilder(Container container, BindingToType binding, InstanceGetterFromSubContainerResolve instanceGetter)
        {
            _container = container;
            _binding = binding;
            _instanceGetter = instanceGetter;
        }

        public BindingToSubcontainerUnderTransformBuilder WithGameObjectName(string name)
        {
            _binding.EnsureCanConfigure();
            _instanceGetter.SubcontainerGetter.ContextGameObjectName = name;
            return new BindingToSubcontainerUnderTransformBuilder(_container, _binding, _instanceGetter);
        }

        public BindingToSubcontainerAsBuilder UnderTransform(Transform parent) => WithGameObjectName(null).UnderTransform(parent);

        public BindingToTypeNonLazyBuilder AsTransient() => UnderTransform(null).AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => UnderTransform(null).AsCached();

        public BindingToTypeAsEntryPointBuilder NonLazy()=> AsCached().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
