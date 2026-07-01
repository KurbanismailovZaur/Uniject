using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
   public class BindingToPoolToBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        public BindingToPoolToBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolFromBuilder<TResult, TResultConcrete, TPool> To<TResultConcrete>() where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new BindingToPoolFromBuilder<TResult, TResultConcrete, TPool>(_container, _binding);
        }
    }
}
