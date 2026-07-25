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

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromConstructor()
        {
            return To<TResult>().FromConstructor();
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromMethod(
            Func<Container, TResult> method)
        {
            return To<TResult>().FromMethod(method);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromMethod(
            Func<Container, InjectContext, TResult> method)
        {
            return To<TResult>().FromMethod(method);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOn(GameObject gameObject)
        {
            return To<TResult>().FromNewComponentOn(gameObject);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(GameObject prefab)
        {
            return To<TResult>().FromComponentInNewPrefab(prefab);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromComponentInNewPrefab(Component prefab)
        {
            return To<TResult>().FromComponentInNewPrefab(prefab);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(GameObject prefab)
        {
            return To<TResult>().FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewPrefab(Component prefab)
        {
            return To<TResult>().FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromNewComponentOnNewGameObject()
        {
            return To<TResult>().FromNewComponentOnNewGameObject();
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromResolve()
        {
            return To<TResult>().FromResolve();
        }

        public BindingToFactoryAsBuilder<TResult, TResult, TFactory> FromFactory<TCustomFactory>()
            where TCustomFactory : CustomFactory<TResult>, new()
        {
            return To<TResult>().FromFactory<TCustomFactory>();
        }

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
