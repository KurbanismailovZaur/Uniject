using System;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToBuilder : BindingBuilder
    {
        public BindingToBuilder(Container container, Binding binding) : base(container, binding) { }

        public BindingFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} is not assignable to {_binding.ContractType}.", nameof(concreteType));

            _binding.ConcreteType = concreteType;
            return new BindingFromBuilder(_container, _binding);
        }

        public BindingFromBuilder To<TConcrete>() => To(typeof(TConcrete));

        public BindingAsBuilder FromConstructor() => To(_binding.ContractType).FromConstructor();

        public BindingAsBuilder FromInstance(object instance) => To(_binding.ContractType).FromInstance(instance);
        
        public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);

        public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To(_binding.ContractType).FromNewComponentOnNewGameObject();
        
        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }

    public class BindingToBuilder<TContract> : BindingBuilder
    {
        public BindingToBuilder(Container container, Binding binding) : base(container, binding) { }

        public BindingFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} is not assignable to {_binding.ContractType}.", nameof(concreteType));

            _binding.ConcreteType = concreteType;
            return new BindingFromBuilder(_container, _binding);
        }

        public BindingFromBuilder To<TConcrete>() where TConcrete : TContract => To(typeof(TConcrete));

        public BindingAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        public BindingAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);
        
        public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
