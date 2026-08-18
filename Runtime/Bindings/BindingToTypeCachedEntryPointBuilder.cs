namespace Uniject.Bindings
{
    public class BindingToTypeCachedEntryPointBuilder : BindingToTypeBuilder
    {
        public BindingToTypeCachedEntryPointBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public void DisposeWithContainer() => _binding.ConfigureDisposeWithContainer();
    }
}
