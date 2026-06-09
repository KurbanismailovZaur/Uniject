using UnityEngine;

namespace Uniject.Bindings
{
    public class BindingToBuilder<TContract>
    {
        private readonly Container _container;
        private readonly Binding _binding;

        public BindingToBuilder(Container container, Binding binding)
        {
            _container = container;
            _binding = binding;
        }

        public BindingFromBuilder To<TConcrete>() where TConcrete : TContract
        {
            _binding.ConcreteType = typeof(TConcrete);
            return new BindingFromBuilder(_container, _binding);
        }

        public BindingAsBuilder FromConstructor() => To<TContract>().FromConstructor();

        public BindingAsBuilder FromInstance(TContract instance) => To<TContract>().FromInstance(instance);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(GameObject prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromComponentInNewPrefab(Component prefab) => To<TContract>().FromComponentInNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(GameObject prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);

        public BindingWithObjectNameBuilder FromNewComponentOnNewPrefab(Component prefab) => To<TContract>().FromNewComponentOnNewPrefab(prefab);
        
        public BindingWithObjectNameBuilder FromNewComponentOnNewGameObject() => To<TContract>().FromNewComponentOnNewGameObject();
        
        public BindingNonLazyBuilder AsTransient() => FromConstructor().AsTransient();

        public BindingNonLazyBuilder AsCached() => FromConstructor().AsCached();
        
        public void NonLazy() => AsTransient().NonLazy();
    }
}
