using System;
using System.Collections.Generic;
using UnityEngine;

namespace Uniject
{
    public interface IObjectBuilder
    {
        void Inject(object instance);

        void Inject(IEnumerable<object> instances);

        T Instantiate<T>();

        object Instantiate(Type concreteType);

        GameObject Instantiate(GameObject prefab);

        TComponent Instantiate<TComponent>(TComponent prefab) where TComponent : Component;

        Component Instantiate(Component prefab);

        TComponent AddComponent<TComponent>(GameObject gameObject) where TComponent : Component;
        
        Component AddComponent(GameObject gameObject, Type componentType);

        TComponent AddComponent<TComponent>(Component component) where TComponent : Component;

        Component AddComponent(Component component, Type componentType);
    }
}
