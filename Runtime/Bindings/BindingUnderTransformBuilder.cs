
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingUnderTransformBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingUnderTransformBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingAsBuilder UnderTransform(Transform parent)
        {
            _binding.ParentTransform = parent;
            return new BindingAsBuilder(_container, _binding);
        }

        public BindingNonLazyBuilder AsTransient() => UnderTransform(null).AsTransient();

        public BindingNonLazyBuilder AsCached() => UnderTransform(null).AsCached();
        
        public void NonLazy() => AsTransient().NonLazy();
    }
}