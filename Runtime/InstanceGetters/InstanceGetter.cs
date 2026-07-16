using System;
using Uniject.Bindings;
using Uniject.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.InstanceGetters
{
    public abstract class InstanceGetter : InstanceGetterBase
    {
        public InstanceGetter(Container container) : base(container) { }

        public abstract object GetInstance(Type concreteType, CreateOptions createOptions);

        protected void SetGameObjectNameAndParent(Component component, CreateOptions createOptions)
        {
            if (createOptions.gameObjectName != null)
                component.gameObject.name = createOptions.gameObjectName;

            if (createOptions.underTransform != null)
                component.transform.SetParent(createOptions.underTransform);
            else if (createOptions.parentForGameObjects != null)
                component.transform.SetParent(createOptions.parentForGameObjects);
            else if (createOptions.context is SceneContext)
                SceneManager.MoveGameObjectToScene(component.gameObject, createOptions.context.gameObject.scene);
        }
    }
}