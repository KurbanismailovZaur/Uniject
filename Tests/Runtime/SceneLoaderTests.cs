using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Uniject.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Uniject.Tests
{
    public class SceneLoaderTests : IPrebuildSetup, IPostBuildCleanup
    {
        private const string SceneName = "SceneLoaderRequested";
        private const string ScenePath =
            "Packages/com.codomaster.uniject/Tests/Runtime/Fixtures/Scenes/SceneLoaderRequested.unity";

        private sealed class ParentDependency
        {
        }

        public void Setup()
        {
#if UNITY_EDITOR
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.path != ScenePath)
                .ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
#endif
        }

        public void Cleanup()
        {
#if UNITY_EDITOR
            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(scene => scene.path != ScenePath)
                .ToArray();
#endif
        }

        [Test]
        public async Task LoadSceneAdditiveAsync_WhenCalledByName_InitializesExpectedSceneContext()
        {
            var parentDependency = new ParentDependency();
            var parentContainer = new Container();
            parentContainer.BindInstance(parentDependency);
            var sceneContainer = default(Container);

            try
            {
                var sceneLoader = parentContainer.Resolve<SceneLoader>();

                await sceneLoader.LoadSceneAdditiveAsync(
                    SceneName,
                    installMethod: container => sceneContainer = container);

                AssertLoadedScene(sceneContainer, parentDependency);
            }
            finally
            {
                await UnloadTestSceneAsync();
            }
        }

        [Test]
        public async Task LoadSceneAdditiveAsync_WhenCalledByBuildIndex_InitializesExpectedSceneContext()
        {
            var sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(ScenePath);
            var parentDependency = new ParentDependency();
            var parentContainer = new Container();
            parentContainer.BindInstance(parentDependency);
            var sceneContainer = default(Container);

            try
            {
                Assert.That(sceneBuildIndex, Is.GreaterThanOrEqualTo(0));

                var sceneLoader = parentContainer.Resolve<SceneLoader>();

                await sceneLoader.LoadSceneAdditiveAsync(
                    sceneBuildIndex,
                    installMethod: container => sceneContainer = container);

                AssertLoadedScene(sceneContainer, parentDependency);
            }
            finally
            {
                await UnloadTestSceneAsync();
            }
        }

        private static void AssertLoadedScene(Container sceneContainer, ParentDependency parentDependency)
        {
            var loadedScene = SceneManager.GetSceneByName(SceneName);

            Assert.That(loadedScene.IsValid(), Is.True);
            Assert.That(loadedScene.isLoaded, Is.True);
            Assert.That(sceneContainer, Is.Not.Null);
            Assert.That(sceneContainer.Context, Is.TypeOf<SceneContext>());
            Assert.That(sceneContainer.Context.gameObject.scene, Is.EqualTo(loadedScene));
            Assert.That(sceneContainer.Context.IsInitialized, Is.True);
            Assert.That(sceneContainer.Resolve<ParentDependency>(), Is.SameAs(parentDependency));
        }

        private static async Task UnloadTestSceneAsync()
        {
            var loadedScene = SceneManager.GetSceneByName(SceneName);

            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
                return;

            await SceneManager.UnloadSceneAsync(loadedScene);
        }
    }
}
