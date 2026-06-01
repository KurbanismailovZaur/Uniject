using System;
using UnityEngine;

namespace Uniject.Getters
{
    public class FromNewComponentOnNewPrefabGetter : InstanceGetter
    {
        private readonly object _prefab;

        public FromNewComponentOnNewPrefabGetter(Container container, object prefab) : base(container)
        {
            if (prefab == null) 
                throw new ArgumentNullException(nameof(prefab), 
                    $"Prefab for {nameof(FromNewComponentOnNewPrefabGetter)} getter can not be null.");

            if (prefab is GameObject gameObject)
                _prefab = gameObject;
            else if (prefab is Component component)
                _prefab = component.gameObject;
            else
                throw new ArgumentException($"Prefab for {nameof(FromNewComponentOnNewPrefabGetter)} must be a GameObject or a Component, but it is not.");
        }

        public override object GetObject(Type concreteType)
        {
            var cloned = UnityEngine.Object.Instantiate(_prefab as Component);
            
            if (cloned.TryGetComponent<InjectionTargets>(out var injectionTargets))
                _container.Inject(injectionTargets);
            
            var script = cloned.gameObject.AddComponent(concreteType);
            _container.Inject(script);

            return script;
        }
    }
}