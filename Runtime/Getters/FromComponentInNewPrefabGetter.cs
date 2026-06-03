using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter<TConcrete> : InstanceGetter
    {
        private readonly TConcrete _prefab;

        private Func<TConcrete, object> _instantiationStrategy;

        public FromComponentInNewPrefabGetter(Container container, TConcrete prefab, Type concreteType) : base(container)
        {
            if (prefab == null) 
                throw new ArgumentNullException(nameof(prefab), 
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter can not be null.");

            _instantiationStrategy = prefab switch
            {
                GameObject => InstantiateGameObject,
                Component => InstantiateGameComponent,
                _ => throw new ArgumentException($"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} must be a GameObject or a Component, but it is {prefab.GetType()}."),
            };
            
            _prefab = prefab;
        }

        private object InstantiateGameObject(TConcrete prefab) => _container.Instantiate(_prefab as GameObject);

        private object InstantiateGameComponent(TConcrete prefab) => _container.Instantiate(_prefab as Component);

        public override object GetObject(Type concreteType) => _instantiationStrategy(_prefab);
    }
}