namespace Uniject.Bindings
{
    public class BindingBuilder
    {
        protected readonly Binding _binding;
        protected readonly Container _container;

        public BindingBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}