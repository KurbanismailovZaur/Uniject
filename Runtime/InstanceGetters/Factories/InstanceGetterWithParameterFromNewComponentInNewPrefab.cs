using System;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters.Factories
{
    public class InstanceGetterWithParameterFromNewComponentInNewPrefab<TParam> : InstanceGetterWithParameter<TParam>
    {
        public InstanceGetterWithParameterFromNewComponentInNewPrefab(Container container, Type paramType, Type concreteType) : base(container)
        {
            if (paramType != typeof(GameObject) && !TypeValidator.TypeIsInterfaceOrComponent(paramType))
                throw new ArgumentException(
                    $"Parameter type {paramType} for {nameof(InstanceGetterWithParameterFromNewComponentInNewPrefab<TParam>)} " +
                    "must be GameObject, Component or interface.",
                    nameof(paramType));

            if (!TypeValidator.TypeCanBeAddedAsComponent(concreteType))
                throw new ArgumentException(
                    $"Concrete type {concreteType} for {nameof(InstanceGetterWithParameterFromNewComponentInNewPrefab<TParam>)} " +
                    "must be a non-abstract Component.",
                    nameof(concreteType));
        }

        public override object GetInstance(
            Type concreteType,
            TParam prefab,
            InjectContext context)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab),
                    $"Prefab for {nameof(InstanceGetterFromComponentInNewPrefab)} can not be null."); 
            
            var prefabGameObject = prefab switch
            {
                GameObject gameObject => gameObject,
                Component component => component.gameObject,
                _ => throw new ArgumentException("Prefab must be GameObject or Component.", nameof(prefab))
            };

            var cloned = Container.Instantiate(prefabGameObject);
            return Container.AddComponent(cloned, concreteType);       
        }
    }
}
