using System;
using Uniject.InstanceGetters;
using Uniject.InstanceGetters.Factories;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
    public class BindingToPoolFromBuilder<TResult, TResultConcrete, TPool> : BindingToPoolBuilder<TResult, TPool> 
        where TResult : class
        where TResultConcrete : TResult
        where TPool : Pool<TResult>, new()
    {
        public BindingToPoolFromBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        private BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new BindingToPoolAsBuilder<TResult, TResultConcrete, TPool>(_container, _binding);
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromConstructor()
        {
            return From(new InstanceGetterFromConstructor(_container));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromMethod(
            Func<Container, TResultConcrete> method)
        {
            return From(new InstanceGetterFromMethod<TResultConcrete>(_container, method));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromComponentInNewPrefab(GameObject prefab)
        {
            return From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromComponentInNewPrefab(Component prefab)
        {
            return From(new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromNewComponentOnNewPrefab(GameObject prefab)
        {
            return From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromNewComponentOnNewPrefab(Component prefab)
        {
            return From(new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromNewComponentOnNewGameObject()
        {
            return From(new InstanceGetterFromNewComponentOnNewGameObject(_container, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromResolve()
        {
            return From(new InstanceGetterFromResolve(_container, _binding.ContractType, _binding.ResultConcreteType));
        }

        public BindingToPoolAsBuilder<TResult, TResultConcrete, TPool> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TResultConcrete>, new()
        {
            return From(new InstanceGetterFromFactory<TResultConcrete, TCustomFactory>(_container));
        }

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
