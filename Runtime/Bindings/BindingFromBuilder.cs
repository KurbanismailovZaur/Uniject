using Uniject.Getters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingFromBuilder<TConcrete>
    {
        private readonly Binding _binding;
        private readonly Container _container;

        public BindingFromBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        private BindingAsBuilder From(InstanceGetter instanceGetter)
        {
            _binding.From(instanceGetter);
            return new BindingAsBuilder(_container, _binding);
        }

        public BindingAsBuilder FromConstructor() => From(new FromConstructorGetter(_container));

        public BindingAsBuilder FromInstance(TConcrete instance) => From(new FromInstanceGetter(_container, instance));

        public BindingAsBuilder FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter<TConcrete>(_container, prefab, _binding.ConcreteType));
        
        public BindingAsBuilder FromComponentInNewPrefab(TConcrete prefab) => From(new FromComponentInNewPrefabGetter<TConcrete>(_container, prefab));

        public BindingAsBuilder FromComponentInNewPrefabResource(string pathToPrefabResource) => From(new FromComponentInNewPrefabResourceGetter(_container, pathToPrefabResource, _binding.ConcreteType));

        public BindingAsBuilder FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        public BindingAsBuilder FromNewComponentOnNewPrefabResource(string pathToPrefabResource) => From(new FromNewComponentOnNewPrefabResourceGetter(_container, pathToPrefabResource, _binding.ConcreteType));

        public BindingAsBuilder FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ConcreteType));
    }
}