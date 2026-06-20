using Uniject.InstanceGetters;
using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToTypeFromBuilder : BindingToTypeBuilder
    {
        public BindingToTypeFromBuilder(Container container, BindingToType binding) : base(container, binding) { }

        private BindingToTypeWithGameObjectNameBuilder From(InstanceGetter instanceGetter)
        {
            _binding.InstanceGetter = instanceGetter;
            return new BindingToTypeWithGameObjectNameBuilder(_container, _binding);
        }

        public BindingToTypeAsBuilder FromConstructor() => From(new FromConstructorGetter(_container)).WithGameObjectName(null).UnderTransform(null);

        public BindingToTypeAsBuilder FromInstance(object instance) => From(new FromInstanceGetter(_container, instance, _binding.ConcreteType)).WithGameObjectName(null).UnderTransform(null);

        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        public BindingToTypeWithGameObjectNameBuilder FromComponentInNewPrefab(Component prefab) => From(new FromComponentInNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));

        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => From(new FromNewComponentOnNewPrefabGetter(_container, prefab, _binding.ConcreteType));
        
        public BindingToTypeWithGameObjectNameBuilder FromNewComponentOnNewGameObject() => From(new FromNewComponentOnNewGameObjectGetter(_container, _binding.ConcreteType));
        
        public BindingToTypeAsBuilder FromResolve() => From(new FromResolveGetter(_container, _binding.ContractType, _binding.ConcreteType)).WithGameObjectName(null).UnderTransform(null);

        public BindingToTypeNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingToTypeNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public BindingToTypeAsEntryPointBuilder NonLazy() => AsTransient().NonLazy();

        public void AsEntryPoint() => NonLazy().AsEntryPoint();
    }
}
