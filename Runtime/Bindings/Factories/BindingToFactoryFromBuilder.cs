using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryFromBuilder<TResult, TResultConcrete, TFactory> : BindingToFactoryBuilder<TResult, TFactory> 
        where TResultConcrete : TResult
        where TFactory : Factory<TResult>, new()
    {
        public BindingToFactoryFromBuilder(Container container, BindingToFactory<TResult, TFactory> binding) : base(container, binding) { }

        private BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory>(_container, _binding);
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromConstructor() => From(new InstanceGetterFromConstructor(_container));

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(GameObject prefab) => From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));
        
        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(Component prefab) => From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(Component prefab) => From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));
        
        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewGameObject() => From(new InstanceGetterFromNewComponentOnNewGameObject(_container, _binding.ResultConcreteType));
        
        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromResolve() => From(new InstanceGetterFromResolve(_container, _binding.ContractType, _binding.ResultConcreteType));
        
        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromFactory<TCustomFactory>() where TCustomFactory : IFactory<TResultConcrete>, new() => From(new InstanceGetterFromFactory<TResultConcrete, TCustomFactory>(_container));

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
