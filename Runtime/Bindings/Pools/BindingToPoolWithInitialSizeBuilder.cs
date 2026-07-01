using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
   public class BindingToPoolWithInitialSizeBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        public BindingToPoolWithInitialSizeBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolWithMaxSizeBuilder<TResult, TPool> WithInitialSize(int initialSize)
        {
            _binding.InitialSize = initialSize;
            return new BindingToPoolWithMaxSizeBuilder<TResult, TPool>(_container, _binding);
        }
    }
}
