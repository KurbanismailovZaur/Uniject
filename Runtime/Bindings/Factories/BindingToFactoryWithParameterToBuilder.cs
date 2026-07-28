using System;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameterToBuilder<TParam, TResult, TFactory> : BindingToFactoryWithParameterBuilder<TParam, TResult, TFactory> 
        where TFactory : Factory<TParam, TResult>, new()
    {
        public BindingToFactoryWithParameterToBuilder(Container container, BindingToFactoryWithParameter<TParam, TResult, TFactory> binding) 
            : base(container, binding) { }

        public BindingToFactoryWithParameterFromBuilder<TParam, TResult, TResultConcrete, TFactory> To<TResultConcrete>() 
            where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new BindingToFactoryWithParameterFromBuilder<TParam, TResult, TResultConcrete, TFactory>(_container, _binding);
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResult, TFactory> FromMethod(
            Func<TParam, InjectContext, TResult> method)
        {
            return To<TResult>().FromMethod(method);
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResult, TFactory> FromComponentInNewPrefab()
        {
            return To<TResult>().FromComponentInNewPrefab();
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResult, TFactory> FromNewComponentOnNewPrefab()
        {
            return To<TResult>().FromNewComponentOnNewPrefab();
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResult, TFactory> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TParam, TResult>, new()
        {
            return To<TResult>().FromFactory<TCustomFactory>();
        }

        public void AsTransient() => FromComponentInNewPrefab().AsTransient();

        public void AsCached() => FromComponentInNewPrefab().AsCached();
    }
}
