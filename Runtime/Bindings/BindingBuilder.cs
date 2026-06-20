namespace Uniject.Bindings
{
    public class BindingBuilder
    {
        protected readonly BindingToType _binding;
        protected readonly Container _container;

        public BindingBuilder(Container container, BindingToType binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}