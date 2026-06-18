using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Factories.Bindings
{
    public class FactoryBindingFromBuilder<TFactory> : FactoryBindingBuilder<TFactory> where TFactory : Factory, new()
    {
        public FactoryBindingFromBuilder(Container container, FactoryBinding<TFactory> binding) : base(container, binding) { }

        private FactoryBindingAsBuilder<TFactory> From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new FactoryBindingAsBuilder<TFactory>(_container, _binding);
        }

        public FactoryBindingAsBuilder<TFactory> FromConstructor() => From(new FromConstructorGetter(_container));

        // public BindingAsBuilder FromInstance(object instance) => From(new FromInstanceGetter(_container, instance, _binding.ConcreteType)).WithGameObjectName(null).UnderTransform(null);

        // public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        // public BindingWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        // public BindingWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ConcreteType));
        
        // public BindingAsBuilder FromResolve() => From(new FromResolveGetter(_container, _binding.ContractType, _binding.ConcreteType)).WithGameObjectName(null).UnderTransform(null);

        // public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        // public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
    }
}
