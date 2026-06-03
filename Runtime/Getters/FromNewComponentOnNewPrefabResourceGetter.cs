using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewPrefabResourceGetter : InstanceGetter
    {
        private readonly string _pathToPrefabResource;

        public FromNewComponentOnNewPrefabResourceGetter(Container container, string pathToPrefabResource, Type concreteType) : base(container)
        {
            if (!typeof(Component).IsAssignableFrom(concreteType))
                throw new ArgumentException($"Type {concreteType} for {nameof(FromNewComponentOnNewPrefabResourceGetter)} " + 
                    "getter must be a Component, but it is not.");

            if (string.IsNullOrWhiteSpace(pathToPrefabResource))
                throw new ArgumentException($"Path to prefab resource for {nameof(FromNewComponentOnNewPrefabResourceGetter)} " + 
                    "getter can not be null or empty.", nameof(pathToPrefabResource));

            _pathToPrefabResource = pathToPrefabResource;
        }

        public override object GetObject(Type concreteType)
        {
            var prefab = Resources.Load<GameObject>(_pathToPrefabResource);

            if (prefab == null)
                throw new ArgumentException($"Prefab resource at path \"{_pathToPrefabResource}\" for " 
                    + $"{nameof(FromNewComponentOnNewPrefabResourceGetter)} does not have a component of type {concreteType}.");

            var cloned = _container.Instantiate(prefab);
            
            var script = cloned.AddComponent(concreteType);
            _container.Inject(script);
            
            return script;
        }
    }
}