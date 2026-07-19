using System;
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

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromConstructor()
        {
            return From(new InstanceGetterFromConstructor(_container));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromMethod(
            Func<Container, TResultConcrete> method)
        {
            return From(new InstanceGetterFromMethod<TResultConcrete>(_container, method));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(GameObject prefab)
        {
            return From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromComponentInNewPrefab(Component prefab)
        {
            return From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(GameObject prefab)
        {
            return From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewPrefab(Component prefab)
        {
            return From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromNewComponentOnNewGameObject()
        {
            return From(new InstanceGetterFromNewComponentOnNewGameObject(_container, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromResolve()
        {
            return From(new InstanceGetterFromResolve(_container, _binding.ContractType, _binding.ResultConcreteType));
        }

        public BindingToFactoryAsBuilder<TResult, TResultConcrete, TFactory> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TResultConcrete>, new()
        {
            return From(new InstanceGetterFromFactory<TResultConcrete, TCustomFactory>(_container));
        }

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
