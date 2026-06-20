using System;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameterToBuilder<TParam, TResult, TFactory> : BindingToFactoryWithParameterBuilder<TParam, TResult, TFactory> 
        where TFactory : Factory, new()
    {
        public BindingToFactoryWithParameterToBuilder(Container container, BindingToFactoryWithParameter<TParam, TResult, TFactory> binding) : base(container, binding) { }

        public BindingToFactoryWithParameterFromBuilder<TParam, TResult, TResultConcrete, TFactory> To<TResultConcrete>() where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new BindingToFactoryWithParameterFromBuilder<TParam, TResult, TResultConcrete, TFactory>(_container, _binding);
        }
        
        // public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab() => To<TResult>().FromComponentInNewPrefab();
        
        // public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab() => To<TResult>().FromNewComponentOnNewPrefab();

        // public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewGameObject() => To<TResult>().FromNewComponentOnNewGameObject();
        
        // public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromFactory<TCustomFactory>() where TCustomFactory : IFactory<TResult>, new() => To<TResult>().FromFactory<TCustomFactory>();

        // public void AsTransient() => FromComponentInNewPrefab().AsTransient();

        // public void AsCached() => FromComponentInNewPrefab().AsCached();
    }
}
