using System;
using Uniject.Lifecycle;

namespace Uniject.Bindings
{
    public class BindingAsEntryPointBuilder
    {
        private readonly BindingToType _binding;
        private readonly Container _container;

        public BindingAsEntryPointBuilder(Container container, BindingToType binding)
        {
            _container = container;
            _binding = binding;
        }

        public void AsEntryPoint()
        {
            if (!typeof(IEntryPoint).IsAssignableFrom(_binding.ConcreteType))
                throw new InvalidOperationException($"Type {_binding.ConcreteType} is not assignable from {typeof(IEntryPoint)}");

            _binding.Scope = Scope.Cached;
            _binding.IsEntryPoint = true;
        }
    }
}
