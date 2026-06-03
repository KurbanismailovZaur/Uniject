using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabGetter<TConcrete> : InstanceGetter
    {
        private readonly Component _prefab;

        private FromComponentInNewPrefabGetter(Container container, object prefab) : base(container)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(FromComponentInNewPrefabGetter<TConcrete>)} getter can not be null.");
        }

        public FromComponentInNewPrefabGetter(Container container, GameObject prefab, Type concreteType) : this(container, prefab)
        {
            _prefab = prefab.GetComponent(concreteType);
        }

        public FromComponentInNewPrefabGetter(Container container, TConcrete prefab) : this(container, (object)prefab)
        {
            _prefab = prefab as Component;
        }
        
        public override object GetObject(Type concreteType) => _container.Instantiate(_prefab);
    }
}