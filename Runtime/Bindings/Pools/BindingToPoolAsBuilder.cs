using Uniject.Bindings;

namespace Uniject.Bindings.Pools
{
    public class BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> : BindingToPoolBuilder<TResult, TPool> 
        where TResultConcrete : TResult
        where TPool : Pool<TResult>, new()
    {
        public BindingToPoolAsBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}