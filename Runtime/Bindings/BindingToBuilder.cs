namespace Uniject.Bindings
{
    public class BindingToBuilder<TContract>
    {
        private readonly Container _container;
        private readonly Binding _binding;

        public BindingToBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingFromBuilder<TConcrete> To<TConcrete>() where TConcrete : TContract
        {
            _binding.To(typeof(TConcrete));
            return new BindingFromBuilder<TConcrete>(_container, _binding);
        }
    }
}