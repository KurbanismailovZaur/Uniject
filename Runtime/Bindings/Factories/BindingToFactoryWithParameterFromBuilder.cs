using System;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
    public class BindingToFactoryWithParameterFromBuilder<TParam, TResult, TResultConcrete, TFactory> 
        : BindingToFactoryWithParameterBuilder<TParam, TResult, TFactory> 
        where TResultConcrete : TResult
        where TFactory : Factory<TParam, TResult>, new()
    {
        public BindingToFactoryWithParameterFromBuilder(Container container, BindingToFactoryWithParameter<TParam, TResult, TFactory> binding) 
            : base(container, binding) { }

        private BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> From(InstanceGetterWithParameter<TParam> instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory>(_container, _binding);
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> FromMethod(
            Func<TParam, InjectContext, TResultConcrete> method)
        {
            return From(new InstanceGetterWithParameterFromMethod<TParam, TResultConcrete>(_container, method));
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> FromComponentInNewPrefab()
        {
            return From(new InstanceGetterWithParameterFromComponentInNewPrefab<TParam>(_container, _binding.ParamType, 
                _binding.ResultConcreteType));
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab()
        {
            return From(new InstanceGetterWithParameterFromNewComponentInNewPrefab<TParam>(_container, _binding.ParamType, 
                _binding.ResultConcreteType));
        }

        public BindingToFactoryWithParameterAsBuilder<TParam, TResult, TResultConcrete, TFactory> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TParam, TResultConcrete>, new()
        {
            return From(new InstanceGetterWithParameterFromFactory<TParam, TResultConcrete, TCustomFactory>(_container));
        }

        public void AsTransient() => FromComponentInNewPrefab().AsTransient();

        public void AsCached() => FromComponentInNewPrefab().AsCached();
    }
}
