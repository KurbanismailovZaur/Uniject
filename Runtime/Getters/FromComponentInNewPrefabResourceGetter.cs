using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabResourceGetter : InstanceGetter 
    {
        private readonly string _pathToPrefabResource;

        public FromComponentInNewPrefabResourceGetter(Container container, string pathToPrefabResource) : base(container)
        {
            if (string.IsNullOrWhiteSpace(pathToPrefabResource))
                throw new ArgumentException("Path to prefab resource for FromComponentInNewPrefabResource getter " + 
                    "can not be null or empty.", nameof(pathToPrefabResource));

            _pathToPrefabResource = pathToPrefabResource;
        }

        public override object GetObject(Type concreteType)
        {
            var prefab = Resources.Load<GameObject>(_pathToPrefabResource).GetComponent(concreteType);
            return _container.InstantiatePrefab(prefab);
        }
    }
}