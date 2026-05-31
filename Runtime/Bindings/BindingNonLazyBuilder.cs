using Uniject.Getters;

namespace Uniject.Bindings
{
    public class BindingNonLazyBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingNonLazyBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public void NonLazy() => _binding.NonLazy();
    }
}