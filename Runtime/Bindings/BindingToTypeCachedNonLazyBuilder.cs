namespace Uniject.Bindings
{
    public class BindingToTypeCachedNonLazyBuilder : BindingToTypeAsEntryPointBuilder
    {
        public BindingToTypeCachedNonLazyBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public new BindingToTypeCachedEntryPointBuilder AsEntryPoint()
        {
            _binding.ConfigureAsEntryPoint();
            return new BindingToTypeCachedEntryPointBuilder(_container, _binding);
        }

        public void DisposeWithContainer() => _binding.ConfigureDisposeWithContainer();
    }
}
