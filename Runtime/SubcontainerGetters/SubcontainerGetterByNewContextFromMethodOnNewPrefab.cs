using System;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.InstanceGetters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.SubcontainerGetters
{
    public class SubcontainerGetterByNewContextFromMethodOnNewPrefab : SubcontainerGetter
    {
        private readonly GameObject _prefab;
        private readonly Action<Container> _installMethod;

        public SubcontainerGetterByNewContextFromMethodOnNewPrefab(Container container, GameObject prefab, Action<Container> installMethod) : base(container)
        {
            _prefab = prefab;
            _installMethod = installMethod;
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

            var gameObjectContext = gameObject.AddComponent<GameObjectContext>();
            gameObjectContext.Initialize(_container);
            gameObjectContext.Install();
            _installMethod?.Invoke(gameObjectContext.Container);
            gameObjectContext.Build();

            return gameObjectContext.Container;
        }
    }
}
