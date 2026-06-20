using System;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
   public class BindingToFactoryToBuilder<TResult, TFactory> : BindingToFactoryBuilder<TResult, TFactory> where TFactory : Factory<TResult>, new()
    {
        public BindingToFactoryToBuilder(Container container, BindingToFactory<TResult, TFactory> binding) : base(container, binding) { }

        public BindingToFactoryFromBuilder<TResult, TResultConcrete, TFactory> To<TResultConcrete>() where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new BindingToFactoryFromBuilder<TResult, TResultConcrete, TFactory>(_container, _binding);
        }
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromConstructor() => To<TResult>().FromConstructor();
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(GameObject prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(Component prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(Component prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewGameObject() => To<TResult>().FromNewComponentOnNewGameObject();
        
        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromResolve() => To<TResult>().FromResolve();

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromFactory<TCustomFactory>() where TCustomFactory : IFactory<TResult>, new() => To<TResult>().FromFactory<TCustomFactory>();

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
