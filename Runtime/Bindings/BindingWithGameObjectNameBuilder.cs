using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingWithGameObjectNameBuilder : BindingBuilder
    {
        public BindingWithGameObjectNameBuilder(Container container, Binding binding) : base(container, binding) { }

        public BindingUnderTransformBuilder WithGameObjectName(string name)
        {
            _binding.ObjectName = name;
            return new BindingUnderTransformBuilder(_container, _binding);
        }

        public BindingAsBuilder UnderTransform(Transform parent) => WithGameObjectName(null).UnderTransform(parent);

        public BindingNonLazyBuilder AsTransient() => WithGameObjectName(null).UnderTransform(null).AsTransient();

        public BindingNonLazyBuilder AsCached() => WithGameObjectName(null).UnderTransform(null).AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}