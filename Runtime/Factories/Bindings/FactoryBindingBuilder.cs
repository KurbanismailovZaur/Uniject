using System;
using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        protected readonly FactoryBinding<TResult, TFactory> _binding;
        protected readonly Container _container;

        public FactoryBindingBuilder(Container container, FactoryBinding<TResult, TFactory> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}