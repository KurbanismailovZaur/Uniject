using System;
using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingBuilder<TFactory> where TFactory : Factory, new()
    {
        protected readonly FactoryBinding<TFactory> _binding;
        protected readonly Container _container;

        public FactoryBindingBuilder(Container container, FactoryBinding<TFactory> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}