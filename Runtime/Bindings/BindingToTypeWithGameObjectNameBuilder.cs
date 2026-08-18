using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeWithGameObjectNameBuilder : BindingToTypeBuilder
    {
        public BindingToTypeWithGameObjectNameBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeUnderTransformBuilder WithGameObjectName(string name)
        {
            _binding.ConfigureObjectName(name);
            return new BindingToTypeUnderTransformBuilder(_container, _binding);
        }

        public BindingToTypeAsBuilder UnderTransform(Transform parent) => WithGameObjectName(null).UnderTransform(parent);

        public BindingToTypeNonLazyBuilder AsTransient() => WithGameObjectName(null).UnderTransform(null).AsTransient();

        public BindingToTypeCachedBuilder AsCached() => WithGameObjectName(null).UnderTransform(null).AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
