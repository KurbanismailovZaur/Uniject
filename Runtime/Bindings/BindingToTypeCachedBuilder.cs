namespace Uniject.Bindings
{
    public class BindingToTypeCachedBuilder : BindingToTypeNonLazyBuilder
    {
        public BindingToTypeCachedBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public new BindingToTypeCachedNonLazyBuilder NonLazy()
        {
            _binding.ConfigureNonLazy();
            return new BindingToTypeCachedNonLazyBuilder(_container, _binding);
        }

        public void DisposeWithContainer() => _binding.ConfigureDisposeWithContainer();
    }
}
