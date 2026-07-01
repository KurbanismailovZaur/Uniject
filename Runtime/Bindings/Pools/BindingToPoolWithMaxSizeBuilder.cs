using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
   public class BindingToPoolWithMaxSizeBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        public BindingToPoolWithMaxSizeBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolExpandTypeBuilder<TResult, TPool> WithMaxSize(int maxSize)
        {
            _binding.MaxSize = maxSize;
            return new BindingToPoolExpandTypeBuilder<TResult, TPool>(_container, _binding);
        }
    }
}
