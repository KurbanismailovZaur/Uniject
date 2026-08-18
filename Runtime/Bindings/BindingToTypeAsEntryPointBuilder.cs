namespace Uniject.Bindings
{
    public class BindingToTypeAsEntryPointBuilder : BindingToTypeBuilder
    {
        public BindingToTypeAsEntryPointBuilder(Container container, BindingToType binding)
            : base(container, binding)
        {
        }

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint()
        {
            _binding.ConfigureAsEntryPoint();
            return new BindingToTypeCachedEntryPointBuilder(_container, _binding);
        }
    }
}
