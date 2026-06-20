using Uniject.Bindings;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> : BindingToFactoryBuilder<TResult, TFactory> 
        where TResultConcrete : TResult
        where TFactory : Factory, new()
    {
        public BindingToFactoryAsBuilder(Container container, BindingToFactory<TResult, TFactory> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}