using Uniject.Bindings;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> : BindingToFactoryWithParameterBuilder<TParam, TResult, TFactory> 
        where TResultConcrete : TResult
        where TFactory : Factory<TParam, TResult>, new()
    {
        public BindingToFactoryWithParameterAsBuilder(Container container, BindingToFactoryWithParameter<TParam, TResult, TFactory> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}