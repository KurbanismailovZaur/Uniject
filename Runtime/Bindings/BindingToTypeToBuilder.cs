using System;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeToBuilder : BindingToTypeBuilder
    {
        public BindingToTypeToBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} is not assignable to {_binding.ContractType}.", nameof(concreteType));

            _binding.ConcreteType = concreteType;
            return new BindingToTypeFromBuilder(_container, _binding);
        }

        public BindingToTypeFromBuilder To<TConcrete>() => To(typeof(TConcrete));

        public BindingToTypeAsBuilder FromConstructor() => To(_binding.ContractType).FromConstructor();

        public BindingToTypeAsBuilder FromInstance(object instance) => To(_binding.ContractType).FromInstance(instance);
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To(_binding.ContractType).FromNewComponentOnNewGameObject();
        
        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }

    public class BindingToBuilder<TContract> : BindingToTypeBuilder
    {
        public BindingToBuilder(Container container, BindingToType binding) : base(container, binding) { }

        public BindingToTypeFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} is not assignable to {_binding.ContractType}.", nameof(concreteType));

            _binding.ConcreteType = concreteType;
            return new BindingToTypeFromBuilder(_container, _binding);
        }

        public BindingToTypeFromBuilder To<TConcrete>() where TConcrete : TContract => To(typeof(TConcrete));

        public BindingToTypeAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        public BindingToTypeAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
