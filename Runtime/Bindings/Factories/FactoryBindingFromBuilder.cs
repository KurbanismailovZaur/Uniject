using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class FactoryBindingFromBuilder<TResult, TFactory> : FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingFromBuilder(Container container, FactoryBinding<TResult, TFactory> binding) : base(container, binding) { }

        private FactoryBindingAsBuilder<TResult, TFactory> From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new FactoryBindingAsBuilder<TResult, TFactory>(_container, _binding);
        }

        public FactoryBindingAsBuilder<TResult, TFactory> FromConstructor() => From(new FromConstructorGetter(_container));

        public FactoryBindingAsBuilder<TResult, TFactory> FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromComponentInNewPrefab(Component prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));

        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));

        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewPrefab(Component prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromResolve() => From(new FromResolveGetter(_container, _binding.ContractType, _binding.ResultConcreteType));

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
