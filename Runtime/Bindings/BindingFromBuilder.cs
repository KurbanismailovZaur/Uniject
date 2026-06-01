using Uniject.Getters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingFromBuilder<T>
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

        public BindingAsBuilder FromInstance(object instance) => From(new FromInstanceGetter(_container, instance));

        public BindingAsBuilder FromComponentInNewPrefab(T prefab) => From(new FromComponentInNewPrefabGetter<T>(_container, prefab));

        public BindingAsBuilder FromComponentInNewPrefabResource(string pathToPrefabResource) => From(new FromComponentInNewPrefabResourceGetter<T>(_container, pathToPrefabResource));
    }
}