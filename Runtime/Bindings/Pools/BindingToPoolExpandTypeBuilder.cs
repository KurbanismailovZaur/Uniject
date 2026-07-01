using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
   public class BindingToPoolExpandTypeBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        public BindingToPoolExpandTypeBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolToBuilder<TResult, TPool> ExpandByOne()
        {
            _binding.ExpandType = ExpandType.ByOne;
            return new BindingToPoolToBuilder<TResult, TPool>(_container, _binding);
        }

        public BindingToPoolToBuilder<TResult, TPool> ExpandByDoubling()
        {
            _binding.ExpandType = ExpandType.ByDoubling;
            return new BindingToPoolToBuilder<TResult, TPool>(_container, _binding);
        }
    }
}
