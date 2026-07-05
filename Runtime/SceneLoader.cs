using System;
using System.IO;
using Uniject.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject
{
    public class SceneLoader
    {
        private readonly Container _container;

        public SceneLoader(Container container) => _container = container;

        public async Awaitable LoadSceneAdditiveAsync(int sceneBuildIndex, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None,
            Action<Container> installMethod = null)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            await LoadSceneAdditiveAsync(sceneName, localPhysicsMode, installMethod);
        }

        public async Awaitable LoadSceneAdditiveAsync(string sceneName, LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None,
            Action<Container> installMethod = null)
        {
            Scene loadedScene = default;
            void OnSceneLoaded(Scene scene, LoadSceneMode mode) => loadedScene = scene;

            SceneManager.sceneLoaded += OnSceneLoaded;
            await SceneManager.LoadSceneAsync(sceneName, new LoadSceneParameters(LoadSceneMode.Additive, localPhysicsMode));
            SceneManager.sceneLoaded -= OnSceneLoaded;

            var rootObjects = loadedScene.GetRootGameObjects();
            foreach (var gameObject in rootObjects)
            {
                if (!gameObject.TryGetComponent<SceneContext>(out var sceneContext))   
                    continue;

                sceneContext.Container.SetParentContainer(_container);
                installMethod?.Invoke(sceneContext.Container);
                break;
            }
        }
    }
}