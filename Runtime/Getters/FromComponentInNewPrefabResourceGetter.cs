using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabResourceGetter<T> : InstanceGetter 
    {
        private readonly string _pathToPrefabResource;

        public FromComponentInNewPrefabResourceGetter(Container container, string pathToPrefabResource) : base(container)
        {
            _pathToPrefabResource = pathToPrefabResource;
        }

        public override object GetObject(Type concreteType)
        {
            var prefab = Resources.Load<GameObject>(_pathToPrefabResource).GetComponent(concreteType);
            return _container.InstantiatePrefab(prefab);
        }
    }
}