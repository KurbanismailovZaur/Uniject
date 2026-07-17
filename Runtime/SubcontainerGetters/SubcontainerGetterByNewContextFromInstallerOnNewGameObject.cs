using System;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.InstanceGetters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByNewContextFromInstallerOnNewGameObject : SubcontainerGetter
    {
        private readonly IInstaller _installer;

        public SubcontainerGetterByNewContextFromInstallerOnNewGameObject(Container container, IInstaller installer) : base(container)
        {
            _installer = installer;
        }

        public override Container GetContainer()
        {
            var gameObject = new GameObject() { name = ContextGameObjectName ?? "GameObjectContext" };
            
            if (ContextUnderTransform != null)
                gameObject.transform.SetParent(ContextUnderTransform);
            else
            {
                var (context, parentTransform) = _container.GetInfoAboutNearestParentForGameObjects();

                if (parentTransform != null)
                    gameObject.transform.SetParent(parentTransform);
                else if (context != null && context is GameObjectContext)
                    gameObject.transform.SetParent(context.transform);
                else if (context != null && context is SceneContext)
                    SceneManager.MoveGameObjectToScene(gameObject, context.gameObject.scene);
            }

            var gameObjectContext = gameObject.AddComponent<GameObjectContext>();
            gameObjectContext.Initialize(_container);
            gameObjectContext.Install();
            _installer.Install(gameObjectContext.Container);
            gameObjectContext.Build();

            return gameObjectContext.Container;
        }
    }
}
