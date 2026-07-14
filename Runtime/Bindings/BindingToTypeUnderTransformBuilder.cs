
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeUnderTransformBuilder : BindingToTypeBuilder
    {
        public BindingToTypeUnderTransformBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeAsBuilder UnderTransform(Transform parent)
        {
            _binding.UnderTransform = parent;
            return new BindingToTypeAsBuilder(_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsTransient() => UnderTransform(null).AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => UnderTransform(null).AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}