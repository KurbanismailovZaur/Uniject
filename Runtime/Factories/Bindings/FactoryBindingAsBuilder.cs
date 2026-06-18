using Uniject.Bindings;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingAsBuilder<TResult> : FactoryBindingBuilder<TResult>
    {
        public FactoryBindingAsBuilder(Container container, FactoryBinding<TResult> binding) : base(container, binding) { }

        public void AsTransient() => _binding.Scope = Scope.Transient;

        public void AsCached() => _binding.Scope = Scope.Cached;
    }
}