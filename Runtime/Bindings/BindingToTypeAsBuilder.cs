namespace Uniject.Bindings
{
    public class BindingToTypeAsBuilder : BindingToTypeBuilder
    {
        public BindingToTypeAsBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeNonLazyBuilder AsTransient()
        {
            _binding.ConfigureScope(Scope.Transient);
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeCachedBuilder AsCached()
        {
            _binding.ConfigureScope(Scope.Cached);
            return new BindingToTypeCachedBuilder(_container, _binding);
        }

        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();
        
        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
