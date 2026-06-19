using Uniject.Bindings;

namespace Uniject.Bindings.Factories
{
    public class FactoryBindingAsBuilder<TResult, TFactory> : FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingAsBuilder(Container container, FactoryBinding<TResult, TFactory> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}