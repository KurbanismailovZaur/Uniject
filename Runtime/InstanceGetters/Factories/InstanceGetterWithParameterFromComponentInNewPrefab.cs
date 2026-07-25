using System;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterWithParameterFromComponentInNewPrefab<TParam> : InstanceGetterWithParameter<TParam>
    {
        public InstanceGetterWithParameterFromComponentInNewPrefab(Container container, Type paramType, Type concreteType) : base(container)
        {
            if (paramType != typeof(GameObject) && !TypeValidator.TypeIsInterfaceOrComponent(paramType))
                throw new ArgumentException($"Parameter type {paramType} for {nameof(InstanceGetterWithParameterFromComponentInNewPrefab<TParam>)} " + 
                    "must be GameObject, Component or interface.", nameof(paramType));

            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException($"Concrete type {concreteType} for {nameof(InstanceGetterFromComponentInNewPrefab)} " + 
                    "must be a Component or an interface.", nameof(concreteType));
        }

        public override object GetInstance(
            Type concreteType,
            TParam prefab,
            InjectContext context)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} can not be null."); 
            
            var prefabComponent = prefab switch
            {
                GameObject gameObject => gameObject.GetComponent(concreteType),
                Component component => concreteType.IsInstanceOfType(component) ? component : component.GetComponent(concreteType),
                _ => throw new ArgumentException("Prefab must be GameObject or Component.", nameof(prefab))
            };

            if (prefabComponent == null)
                throw new ArgumentException($"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} must have a " + 
                    $"component assignable to type {concreteType}.", nameof(prefab));

            return _container.Instantiate(prefabComponent);
        }
    }
}
