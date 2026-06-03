using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromComponentInNewPrefabResourceGetter : InstanceGetter 
    {
        private readonly string _pathToPrefabResource;

        public FromComponentInNewPrefabResourceGetter(Container container, string pathToPrefabResource, Type concreteType) : base(container)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromComponentInNewPrefabResourceGetter)} " + 
                    "getter must be a Component, but it is not.");

            if (string.IsNullOrWhiteSpace(pathToPrefabResource))
                throw new ArgumentException($"Path to prefab resource for {nameof(FromComponentInNewPrefabResourceGetter)} " + 
                    "getter can not be null or empty.", nameof(pathToPrefabResource));

            _pathToPrefabResource = pathToPrefabResource;
        }

        public override object GetObject(Type concreteType)
        {
            var prefab = (Component)Resources.Load(_pathToPrefabResource, concreteType);

            if (prefab == null)
                throw new ArgumentException($"Prefab resource at path \"{_pathToPrefabResource}\" for " 
                    + $"{nameof(FromComponentInNewPrefabResourceGetter)} does not have a component of type {concreteType}.");

            return _container.Instantiate(prefab);
        }
    }
}