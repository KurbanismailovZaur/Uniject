using System;
using Uniject.Exceptions;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToBuilder
    {
        protected readonly Container _container;
        protected readonly Binding _binding;

        public BindingToBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new BindingException($"Type {concreteType} is not assignable to {_binding.ContractType}.");

            _binding.ConcreteType = concreteType;
            return new BindingFromBuilder(_container, _binding);
        }

        public BindingFromBuilder To<TConcrete>() => To(typeof(TConcrete));

        public BindingAsBuilder FromConstructor() => To(_binding.ContractType).FromConstructor();

        public BindingAsBuilder FromInstance(object instance) => To(_binding.ContractType).FromInstance(instance);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);

        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewGameObject() => To(_binding.ContractType).FromNewComponentOnNewGameObject();
        
        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }

    public class BindingToBuilder<TContract>
    {
        protected readonly Container _container;
        protected readonly Binding _binding;

        public BindingToBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingFromBuilder To(Type concreteType)
        {
            if (concreteType == null)
                throw new ArgumentNullException(nameof(concreteType));

            if (!_binding.ContractType.IsAssignableFrom(concreteType))
                throw new BindingException($"Type {concreteType} is not assignable to {_binding.ContractType}.");

            _binding.ConcreteType = concreteType;
            return new BindingFromBuilder(_container, _binding);
        }

        public BindingFromBuilder To<TConcrete>() where TConcrete : TContract => To(typeof(TConcrete));

        public BindingAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        public BindingAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
