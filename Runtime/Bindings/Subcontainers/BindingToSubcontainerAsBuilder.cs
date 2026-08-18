using Uniject.InstanceGetters;

namespace Uniject.Bindings
{
    public class BindingToSubcontainerAsBuilder
    {
        protected readonly Container _container;
        protected readonly BindingToType _binding;
        protected readonly InstanceGetterFromSubContainerResolve _instanceGetter;

        public BindingToSubcontainerAsBuilder(Container container, BindingToType binding, InstanceGetterFromSubContainerResolve instanceGetter)
        {
            _container = container;
            _binding = binding;
            _instanceGetter = instanceGetter;
        }

        public BindingToTypeNonLazyBuilder AsTransient()
        {
            _binding.EnsureCanConfigure();
            _instanceGetter.Scope = Scope.Transient;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsCached()
        {
            _binding.EnsureCanConfigure();
            _instanceGetter.Scope = Scope.Cached;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();
        
        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
