using System;
using UnityEngine;

namespace Uniject.Factories.Bindings
{
   public class FactoryBindingToBuilder<TFactory> : FactoryBindingBuilder<TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingToBuilder(Container container, FactoryBinding<TFactory> binding) : base(container, binding) { }

        public FactoryBindingFromBuilder<TFactory> To(Type resultConcreteType)
        {
            if (resultConcreteType == null)
                throw new ArgumentNullException(nameof(resultConcreteType));

            if (!_binding.ResultContractType.IsAssignableFrom(resultConcreteType))
                throw new ArgumentException($"Type {_binding.ResultContractType} is not assignable from {resultConcreteType} in factory of type {_binding.ContractType}.", nameof(resultConcreteType));

            _binding.ResultConcreteType = resultConcreteType;
            return new FactoryBindingFromBuilder<TFactory>(_container, _binding);
        }

        public FactoryBindingFromBuilder<TFactory> To<TResultConcrete>() => To(typeof(TResultConcrete));

        // public BindingAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        // public BindingAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);
        
        // public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        // public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        // public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        // public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
    }
}
