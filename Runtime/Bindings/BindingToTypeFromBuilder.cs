using System;
using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeFromBuilder : BindingToTypeBuilder
    {
        public BindingToTypeFromBuilder(Container container, BindingToType binding) 
            : base(container, binding) { }

        public BindingToTypeAsBuilder FromConstructor()
        {
            _binding.ConfigureInstanceGetter(new InstanceGetterFromConstructor(_container));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromMethod<TResult>(Func<InjectContext, TResult> method)
        {
            _binding.ConfigureInstanceGetter(new InstanceGetterFromMethod<TResult>(_container, method));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromInstance(object instance)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromInstance(_container, instance, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromNewComponentOn(GameObject gameObject)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOn(_container, gameObject, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromNewComponentOnRoot()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOnRoot(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromNewComponentOnConsumer()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOnConsumer(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromComponentOnConsumer()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentOnConsumer(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromComponentInParents()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentInParents(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromComponentInChildren()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentInChildren(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromComponentInHierarchy()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentInHierarchy(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab)
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromNewComponentOnNewGameObject(_container, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromResolve()
        {
            _binding.ConfigureInstanceGetter(
                new InstanceGetterFromResolve(_container, _binding.ContractType, _binding.ConcreteType));
            return new (_container, _binding);
        }

        public BindingToTypeByBuilder FromSubcontainerResolve()
        {
            _binding.ConfigureInstanceGetter(new InstanceGetterFromSubContainerResolve(_container));
            return new (_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeCachedBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public BindingToTypeCachedEntryPointBuilder AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
