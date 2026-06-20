namespace Uniject.Bindings
{
    public class BindingToTypeAsBuilder : BindingToTypeBuilder
    {
        public BindingToTypeAsBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeNonLazyBuilder AsTransient()
        {
            _binding.Scope = Scope.Transient;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsCached()
        {
            _binding.Scope = Scope.Cached;
            return new BindingToTypeNonLazyBuilder(_container, _binding);
        }

        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();
        
        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}