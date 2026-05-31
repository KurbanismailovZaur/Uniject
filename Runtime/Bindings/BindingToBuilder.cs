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

        public BindingFromBuilder To<T>()
        {
            _binding.To(typeof(T));
            return new BindingFromBuilder(_container, _binding);
        }
    }
}