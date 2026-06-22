namespace Uniject.Bindings
{
    public class BindingToTypeBuilder
    {
        protected readonly Container _container;
        protected readonly BindingToType _binding;

        public BindingToTypeBuilder(Container container, BindingToType binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}