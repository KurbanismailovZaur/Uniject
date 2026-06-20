using System;
using Uniject.Bindings;

namespace Uniject.Bindings.Factories
{
    public class FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        protected readonly BindingToFactory<TResult, TFactory> _binding;
        protected readonly Container _container;

        public FactoryBindingBuilder(Container container, BindingToFactory<TResult, TFactory> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}