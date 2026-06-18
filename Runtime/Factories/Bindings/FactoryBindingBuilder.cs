using System;
using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingBuilder<TResult>
    {
        protected readonly FactoryBinding<TResult> _binding;
        protected readonly Container _container;

        public FactoryBindingBuilder(Container container, FactoryBinding<TResult> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}