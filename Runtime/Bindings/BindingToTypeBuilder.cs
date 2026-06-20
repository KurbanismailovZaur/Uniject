namespace Uniject.Bindings
{
    public class BindingToTypeBuilder
    {
        protected readonly BindingToType _binding;
        protected readonly Container _container;

        public BindingToTypeBuilder(Container container, BindingToType binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}