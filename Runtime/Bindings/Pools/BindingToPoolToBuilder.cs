using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
    public class BindingToPoolToBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> 
        where TResult : class
        where TPool : Pool<TResult>, new()
    {
        public BindingToPoolToBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolFromBuilder<TResult, TResultConcrete, TPool> To<TResultConcrete>() where TResultConcrete : TResult
        {
            _binding.ResultConcreteType = typeof(TResultConcrete);
            return new BindingToPoolFromBuilder<TResult, TResultConcrete, TPool>(_container, _binding);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromConstructor()
        {
            return To<TResult>().FromConstructor();
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromMethod(
            Func<Container, TResult> method)
        {
            return To<TResult>().FromMethod(method);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromComponentInNewPrefab(GameObject prefab)
        {
            return To<TResult>().FromComponentInNewPrefab(prefab);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromComponentInNewPrefab(Component prefab)
        {
            return To<TResult>().FromComponentInNewPrefab(prefab);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromNewComponentOnNewPrefab(GameObject prefab)
        {
            return To<TResult>().FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromNewComponentOnNewPrefab(Component prefab)
        {
            return To<TResult>().FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromNewComponentOnNewGameObject()
        {
            return To<TResult>().FromNewComponentOnNewGameObject();
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromResolve()
        {
            return To<TResult>().FromResolve();
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TResult>, new()
        {
            return To<TResult>().FromFactory<TCustomFactory>();
        }

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
