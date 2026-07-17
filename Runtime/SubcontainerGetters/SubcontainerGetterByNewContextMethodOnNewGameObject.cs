using System;
using Uniject.Contexts;
using Uniject.InstanceGetters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByNewContextMethodOnNewGameObject : SubcontainerGetter
    {
        private readonly Action<Container> _installMethod;

        public SubcontainerGetterByNewContextMethodOnNewGameObject(Container container, Action<Container> installMethod) : base(container)
        {
            _installMethod = installMethod;
        }

        public override Container GetContainer()
        {
            var gameObject = new GameObject() { name = ContextGameObjectName ?? "GameObjectContext" };
            var (context, parentTransform) = _container.GetInfoAboutNearestParentForGameObjects();
            
            if (parentTransform != null)
                gameObject.transform.SetParent(parentTransform);
            else if (context != null && context is GameObjectContext)
                gameObject.transform.SetParent(context.transform);
            else if (context != null && context is SceneContext)
                SceneManager.MoveGameObjectToScene(gameObject, context.gameObject.scene);

            var gameObjectContext = gameObject.AddComponent<GameObjectContext>();
            gameObjectContext.Initialize(_container);
            gameObjectContext.Install();
            _installMethod.Invoke(gameObjectContext.Container);
            
            return gameObjectContext.Container;
        }
    }
}
