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
            _binding.InstanceGetter = new InstanceGetterFromConstructor(_container);
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromMethod<TResult>(Func<InjectContext, TResult> method)
        {
            _binding.InstanceGetter = new InstanceGetterFromMethod<TResult>(_container, method);
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromInstance(object instance)
        {
            _binding.InstanceGetter = new InstanceGetterFromInstance(_container, instance, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromNewComponentOn(GameObject gameObject)
        {
            _binding.InstanceGetter = new InstanceGetterFromNewComponentOn(_container, gameObject, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromNewComponentOnConsumer()
        {
            _binding.InstanceGetter =
                new InstanceGetterFromNewComponentOnConsumer(_container, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab)
        {
            _binding.InstanceGetter = new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab)
        {
            _binding.InstanceGetter = new InstanceGetterFromComponentInNewPrefab(_container, prefab, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab)
        {
            _binding.InstanceGetter = new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab)
        {
            _binding.InstanceGetter = new InstanceGetterFromNewComponentOnNewPrefab(_container, prefab, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject()
        {
            _binding.InstanceGetter = new InstanceGetterFromNewComponentOnNewGameObject(_container, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeAsBuilder FromResolve()
        {
            _binding.InstanceGetter = new InstanceGetterFromResolve(_container, _binding.ContractType, _binding.ConcreteType);
            return new (_container, _binding);
        }

        public BindingToTypeByBuilder FromSubcontainerResolve()
        {
            _binding.InstanceGetter = new InstanceGetterFromSubContainerResolve(_container);
            return new (_container, _binding);
        }

        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
