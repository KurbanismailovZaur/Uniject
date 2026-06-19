using System;
using UnityEngine;

namespace Uniject.Bindings.Factories
{
   public class FactoryBindingToBuilder<TResult, TFactory> : FactoryBindingBuilder<TResult, TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingToBuilder(Container container, FactoryBinding<TResult, TFactory> binding) : base(container, binding) { }

        public FactoryBindingFromBuilder<TResult, TFactory> To(Type resultConcreteType)
        {
            if (resultConcreteType == null)
                throw new ArgumentNullException(nameof(resultConcreteType));

            if (!_binding.ResultContractType.IsAssignableFrom(resultConcreteType))
                throw new ArgumentException($"Type {_binding.ResultContractType} is not assignable from {resultConcreteType} in factory of type {_binding.ContractType}.", nameof(resultConcreteType));

            _binding.ResultConcreteType = resultConcreteType;
            return new FactoryBindingFromBuilder<TResult, TFactory>(_container, _binding);
        }

        public FactoryBindingFromBuilder<TResult, TFactory> To<TResultConcrete>() where TResultConcrete : TResult => To(typeof(TResultConcrete));

        public FactoryBindingAsBuilder<TResult, TFactory> FromConstructor() => To<TResult>().FromConstructor();
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromComponentInNewPrefab(GameObject prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromComponentInNewPrefab(Component prefab) => To<TResult>().FromComponentInNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewPrefab(GameObject prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);

        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewPrefab(Component prefab) => To<TResult>().FromNewComponentOnNewPrefab(prefab);
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromNewComponentOnNewGameObject() => To<TResult>().FromNewComponentOnNewGameObject();
        
        public FactoryBindingAsBuilder<TResult, TFactory> FromResolve() => To<TResult>().FromResolve();

        public void AsTransient() => FromConstructor().AsTransient();

        public void AsCached() => FromConstructor().AsCached();
    }
}
