using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
   public class BindingToPoolWithInitialSizeBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> where TPool : Pool<TResult>, new()
    {
        public BindingToPoolWithInitialSizeBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolWithMaxSizeBuilder<TResult, TPool> WithInitialSize(int initialSize)
        {
            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size can not be less than zero.");

            _binding.InitialSize = initialSize;
            return new BindingToPoolWithMaxSizeBuilder<TResult, TPool>(_container, _binding);
        }

        public BindingToPoolExpandTypeBuilder<TResult, TPool> WithMaxSize(int maxSize)
        {
            return WithInitialSize(0).WithMaxSize(maxSize);
        }

        public BindingToPoolToBuilder<TResult, TPool> ExpandByDoubling()
        {
            return WithMaxSize(-1).ExpandByDoubling();
        }

        public BindingToPoolToBuilder<TResult, TPool> ExpandByOne()
        {
            return WithMaxSize(-1).ExpandByOne();
        }

        public BindingToPoolFromBuilder<TResult, TResultConcrete, TPool> To<TResultConcrete>() where TResultConcrete : TResult
        {
            return ExpandByDoubling().To<TResultConcrete>();
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromConstructor()
        {
            return To<TResult>().FromConstructor();
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
