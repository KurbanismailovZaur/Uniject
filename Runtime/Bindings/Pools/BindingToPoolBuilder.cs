using System;
using Uniject.Bindings;

namespace Uniject.Bindings.Pools
{
    public class BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        protected readonly BindingToPool<TResult, TPool> _binding;
        protected readonly Container _container;

        public BindingToPoolBuilder(Container container, BindingToPool<TResult, TPool> binding)
        {
            _container = container;
            _binding = binding;
        }
    }
}