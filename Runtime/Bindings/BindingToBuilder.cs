namespace Uniject.Bindings
{
    public class BindingToBuilder
    {
        private readonly Container _container;
        private readonly Binding _binding;

        public BindingToBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingFromBuilder<T> To<T>()
        {
            _binding.To(typeof(T));
            return new BindingFromBuilder<T>(_container, _binding);
        }
    }
}