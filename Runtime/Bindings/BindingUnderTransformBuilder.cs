
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingUnderTransformBuilder : BindingBuilder
    {
        public BindingUnderTransformBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingAsBuilder UnderTransform(Transform parent)
        {
            _binding.ParentTransform = parent;
            return new BindingAsBuilder(_container, _binding);
        }

        public BindingNonLazyBuilder AsTransient() => UnderTransform(null).AsTransient();

        public BindingNonLazyBuilder AsCached() => UnderTransform(null).AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}