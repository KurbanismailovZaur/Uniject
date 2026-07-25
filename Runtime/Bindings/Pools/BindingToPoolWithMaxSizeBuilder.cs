using System;
using UnityEngine;

namespace Uniject.Bindings.Pools
{
    public class BindingToPoolWithMaxSizeBuilder<TResult, TPool> : BindingToPoolBuilder<TResult, TPool> 
        where TResult : class
        where TPool : Pool<TResult>, new()
    {
        public BindingToPoolWithMaxSizeBuilder(Container container, BindingToPool<TResult, TPool> binding) : base(container, binding) { }

        public BindingToPoolExpandTypeBuilder<TResult, TPool> WithMaxSize(int maxSize)
        {
            if (maxSize < -1)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size can not be less than -1.");

            _binding.MaxSize = maxSize;
            return new BindingToPoolExpandTypeBuilder<TResult, TPool>(_container, _binding);
        }

        public BindingToPoolWithoutGameObjectActivationBuilder<TResult, TPool> ExpandByDoubling()
        {
            return WithMaxSize(-1).ExpandByDoubling();
        }

        public BindingToPoolWithoutGameObjectActivationBuilder<TResult, TPool> ExpandByOne()
        {
            return WithMaxSize(-1).ExpandByOne();
        }

        public BindingToPoolToBuilder<TResult, TPool> WithoutGameObjectActivation()
        {
            return ExpandByDoubling().WithoutGameObjectActivation();
        }

        public BindingToPoolFromBuilder<TResult, TResultConcrete, TPool> To<TResultConcrete>() where TResultConcrete : TResult
        {
            return ExpandByDoubling().To<TResultConcrete>();
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

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromMethod(
            Func<Container, InjectContext, TResult> method)
        {
            return To<TResult>().FromMethod(method);
        }

        public BindingToPoolAsBuilder<TResult, TResult, TPool> FromNewComponentOn(GameObject gameObject)
        {
            return To<TResult>().FromNewComponentOn(gameObject);
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
