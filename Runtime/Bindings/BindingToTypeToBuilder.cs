using System;
using Uniject.InstanceGetters;
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

            _binding.ConfigureConcreteType(concreteType);
            return new BindingToTypeFromBuilder(_container, _binding);
        }

        public BindingToTypeFromBuilder To<TConcrete>()
        {
            return To(typeof(TConcrete));
        }

        public BindingToTypeAsBuilder FromConstructor()
        {
            return To(_binding.ContractType).FromConstructor();
        }

        public BindingToTypeAsBuilder FromMethod<TResult>(Func<InjectContext, TResult> method)
        {
            return To(_binding.ContractType).FromMethod(method);
        }

        public BindingToTypeAsBuilder FromInstance(object instance)
        {
            return To(_binding.ContractType).FromInstance(instance);
        }

        public BindingToTypeAsBuilder FromNewComponentOn(GameObject gameObject)
        {
            return To(_binding.ContractType).FromNewComponentOn(gameObject);
        }

        public BindingToTypeAsBuilder FromNewComponentOnRoot()
        {
            return To(_binding.ContractType).FromNewComponentOnRoot();
        }

        public BindingToTypeAsBuilder FromNewComponentOnConsumer()
        {
            return To(_binding.ContractType).FromNewComponentOnConsumer();
        }

        public BindingToTypeAsBuilder FromComponentOnConsumer()
        {
            return To(_binding.ContractType).FromComponentOnConsumer();
        }

        public BindingToTypeAsBuilder FromComponentInParents()
        {
            return To(_binding.ContractType).FromComponentInParents();
        }

        public BindingToTypeAsBuilder FromComponentInChildren()
        {
            return To(_binding.ContractType).FromComponentInChildren();
        }

        public BindingToTypeAsBuilder FromComponentInHierarchy()
        {
            return To(_binding.ContractType).FromComponentInHierarchy();
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab)
        {
            return To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab)
        {
            return To(_binding.ContractType).FromComponentInNewPrefab(prefab);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab)
        {
            return To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab)
        {
            return To(_binding.ContractType).FromNewComponentOnNewPrefab(prefab);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject()
        {
            return To(_binding.ContractType).FromNewComponentOnNewGameObject();
        }

        public BindingToTypeByBuilder FromSubcontainerResolve()
        {
            return To(_binding.ContractType).FromSubcontainerResolve();
        }

        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeCachedBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
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

            _binding.ConfigureConcreteType(concreteType);
            return new BindingToTypeFromBuilder(_container, _binding);
        }

        public BindingToTypeFromBuilder To<TConcrete>() where TConcrete : TContract => To(typeof(TConcrete));

        public BindingToTypeAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        public BindingToTypeAsBuilder FromMethod(Func<InjectContext, TContract> method) =>
            To<TContract>().FromMethod(method);

        public BindingToTypeAsBuilder FromResolveGetter<TResolve>(Func<TResolve, TContract> getter)
        {
            _binding.ConfigureConcreteType(typeof(TContract));
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromResolveGetter<TResolve, TContract>(_container, getter));
            return new BindingToTypeAsBuilder(_container, _binding);
        }

        public BindingToTypeAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);

        public BindingToTypeAsBuilder FromNewComponentOn(GameObject gameObject) => To<TContract>().FromNewComponentOn(gameObject);

        public BindingToTypeAsBuilder FromNewComponentOnRoot() => To<TContract>().FromNewComponentOnRoot();

        public BindingToTypeAsBuilder FromNewComponentOnConsumer() => To<TContract>().FromNewComponentOnConsumer();

        public BindingToTypeAsBuilder FromComponentOnConsumer() => To<TContract>().FromComponentOnConsumer();

        public BindingToTypeAsBuilder FromComponentInParents() => To<TContract>().FromComponentInParents();

        public BindingToTypeAsBuilder FromComponentInChildren() => To<TContract>().FromComponentInChildren();

        public BindingToTypeAsBuilder FromComponentInHierarchy() => To<TContract>().FromComponentInHierarchy();

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        public BindingToTypeByBuilder FromSubcontainerResolve()
        {
            return To(_binding.ContractType).FromSubcontainerResolve();
        }

        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeCachedBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
