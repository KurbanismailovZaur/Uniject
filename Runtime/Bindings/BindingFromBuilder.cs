using Uniject.Getters;

namespace Uniject.Bindings
{
    public class BindingFromBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingFromBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingAsBuilder FromConstructor()
        {
            _binding.From(new FromConstructorGetter(_container));
            return new BindingAsBuilder(_container, _binding);
        }

        public BindingAsBuilder FromInstance(object instance)
        {
            _binding.From(new FromInstanceGetter(_container, instance));
            return new BindingAsBuilder(_container, _binding);
        }
    }
}