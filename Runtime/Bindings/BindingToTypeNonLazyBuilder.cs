namespace Uniject.Bindings
{
    public class BindingToTypeNonLazyBuilder : BindingToTypeBuilder
    {
        public BindingToTypeNonLazyBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeAsEntryPointBuilder NonLazy()
        {
            _binding.IsNonLazy = true;
            return new BindingToTypeAsEntryPointBuilder(_container, _binding);
        }
    }
}