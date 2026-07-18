using System;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.InstanceGetters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByContextOnNewPrefab : SubcontainerGetter
    {
        private readonly GameObject _prefab;

        public SubcontainerGetterByContextOnNewPrefab(Container container, GameObject prefab) : base(container)
        {
            _prefab = prefab;
        }

        public override Container GetContainer()
        {
            var gameObject = UnityEngine.Object.Instantiate(_prefab);
            gameObject.name =  ContextGameObjectName ?? "GameObjectContext";
            
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

            var gameObjectContext = gameObject.GetComponent<GameObjectContext>();

            if (gameObjectContext == null)
                throw new ArgumentException("Prefab must contain GameObjectContext on the root.");

            gameObjectContext.Initialize(_container);
            gameObjectContext.Install();
            gameObjectContext.Build();

            return gameObjectContext.Container;
        }
    }
}
