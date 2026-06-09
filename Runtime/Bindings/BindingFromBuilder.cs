using Uniject.Getters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingFromBuilder
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingFromBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        private BindingWithObjectNameBuilder From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new BindingWithObjectNameBuilder(_container, _binding);
        }

        public BindingAsBuilder FromConstructor() => From(new FromConstructorGetter(_container)).WithObjectName(null).UnderTransform(null);

        public BindingAsBuilder FromInstance(object instance) => From(new FromInstanceGetter(_container, instance, _binding.ConcreteType)).WithObjectName(null).UnderTransform(null);

        public BindingWithObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(Component prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ConcreteType));
        
        public BindingAsBuilder FromResolve() => From(new FromResolveGetter(_container, _binding.ContractType, _binding.ConcreteType)).WithObjectName(null).UnderTransform(null);

        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public void NonLazy() => AsTransient().NonLazy();
    }
}
