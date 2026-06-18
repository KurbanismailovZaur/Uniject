using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingAsBuilder<TFactory> : FactoryBindingBuilder<TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingAsBuilder(Container container, FactoryBinding<TFactory> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}