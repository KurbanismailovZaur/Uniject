namespace Uniject.Bindings
{
    public class BindingAsBuilder : BindingBuilder
    {
        public BindingAsBuilder(Container container, Binding binding) : base(container, binding) { }

        public BindingNonLazyBuilder AsTransient()
        {
            _binding.Scope = Scope.Transient;
            return new BindingNonLazyBuilder(_container, _binding);
        }

        public BindingNonLazyBuilder AsCached()
        {
            _binding.Scope = Scope.Cached;
            return new BindingNonLazyBuilder(_container, _binding);
        }

        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();
        
        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}