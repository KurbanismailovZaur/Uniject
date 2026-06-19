using System;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
   public class FactoryBindingToBuilder<TResult, TFactory> : FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingToBuilder(Container container, FactoryBinding<TResult, TFactory> binding) : base(container, binding) { }

        public FactoryBindingFromBuilder<TResult, TResultConcrete, TFactory> To<TResultConcrete>() where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new FactoryBindingFromBuilder<TResult, TResultConcrete, TFactory>(_container, _binding);
        }
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromConstructor() => To<TResult>().FromConstructor();
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(GameObject prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(Component prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);

        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(Component prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewGameObject() => To<TResult>().FromNewComponentOnNewGameObject();
        
        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromResolve() => To<TResult>().FromResolve();

        public FactoryBindingAsBuilder<TResult, TResult, TFactory> FromFactory<TCustomFactory>() where TCustomFactory : IFactory<TResult>, new() => To<TResult>().FromFactory<TCustomFactory>();

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
