using System;
using Uniject.Bindings;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryBuilder<TResult, TFactory> where TFactory : Factory<TResult>, new()
    {
        protected readonly BindingToFactory<TResult, TFactory> _binding;
        protected readonly Container _container;

        public BindingToFactoryBuilder(Container container, BindingToFactory<TResult, TFactory> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}