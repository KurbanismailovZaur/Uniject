using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class FactoryBindingFromBuilder<TResult, TResultConcrete, TFactory> : FactoryBindingBuilder<TResult, TFactory> 
        where TResultConcrete : TResult
        where TFactory : Factory, new()
    {
        public FactoryBindingFromBuilder(Container container, FactoryBinding<TResult, TFactory> binding) : base(container, binding) { }

        private FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory>(_container, _binding);
        }

        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromConstructor() => From(new FromConstructorGetter(_container));

        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(Component prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));

        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));

        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(Component prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromResolve() => From(new FromResolveGetter(_container, _binding.ContractType, _binding.ResultConcreteType));
        
        public FactoryBindingAsBuilder<TResult, TResultConcrete, TFactory> FromFactory<TCustomFactory>() where TCustomFactory : IFactory<TResultConcrete>, new() => From(new FromFactoryGetter<TResultConcrete, TCustomFactory>(_container));

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
