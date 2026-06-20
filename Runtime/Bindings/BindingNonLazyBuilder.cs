namespace Uniject.Bindings
{
    public class BindingNonLazyBuilder : BindingBuilder
    {
        public BindingNonLazyBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingAsEntryPointBuilder NonLazy()
        {
            _binding.IsNonLazy = true;
            return new BindingAsEntryPointBuilder(_container, _binding);
        }
    }
}