using Uniject.Getters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingWithObjectNameBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingWithObjectNameBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingUnderTransformBuilder WithObjectName(string name)
        {
            _binding.ObjectName = name;
            return new BindingUnderTransformBuilder(_container, _binding);
        }

        public BindingAsBuilder UnderTransform(Transform parent) => WithObjectName(null).UnderTransform(parent);

        public BindingNonLazyBuilder AsTransient() => WithObjectName(null).UnderTransform(null).AsTransient();

        public BindingNonLazyBuilder AsCached() => WithObjectName(null).UnderTransform(null).AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}