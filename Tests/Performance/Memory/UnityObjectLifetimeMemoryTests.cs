using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Contexts;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.Memory
{
    public class UnityObjectLifetimeMemoryTests
    {
        private Scene _scene;
        private GameObject _root;
        private const int ObjectCount = 16;
        private const int Cycles = 8;

        [UnityTest, Performance]
        public IEnumerator GameObjectContext_Destroy_ReleasesCreatedUnityObjects()
        {
            RequirePlayMode();
            var created = new LifetimeComponent[ObjectCount];
            var nativeGroup = new SampleGroup("CreatedComponents.NativeBytes", SampleUnit.Byte);
            var destroyedGroup = new SampleGroup("DestroyedComponents", SampleUnit.Undefined);

            for (var cycle = 0; cycle < Cycles; cycle++)
            {
                _root = new GameObject("Memory lifetime context");
                var context = _root.AddComponent<ContextLifecycleMemoryTests.MemoryGameObjectContext>();
                context.Configure();
                context.Run();
                CreateComponents(context.Container, created);
                for (var i = 0; i < created.Length; i++)
                    Assert.That(created[i].transform.parent, Is.EqualTo(_root.transform));
                RecordComponentNativeBytes(created, nativeGroup);

                Object.Destroy(_root);
                yield return null;

                Assert.That(_root == null, Is.True);
                AssertDestroyed(created);
                Measure.Custom(destroyedGroup, ObjectCount);
                _root = null;
            }
        }

        [UnityTest, Performance]
        public IEnumerator SceneContext_Unload_ReleasesCreatedUnityObjects()
        {
            RequirePlayMode();
            var created = new LifetimeComponent[ObjectCount];
            var nativeGroup = new SampleGroup("SceneComponents.NativeBytes", SampleUnit.Byte);
            var destroyedGroup = new SampleGroup("UnloadedComponents", SampleUnit.Undefined);

            for (var cycle = 0; cycle < Cycles; cycle++)
            {
                _scene = SceneManager.CreateScene("Uniject memory " + Guid.NewGuid().ToString("N"));
                _root = new GameObject("Memory scene context");
                SceneManager.MoveGameObjectToScene(_root, _scene);
                var context = _root.AddComponent<ContextLifecycleMemoryTests.MemorySceneContext>();
                context.Configure(new List<GameObjectContext>());
                context.Run();
                CreateComponents(context.Container, created);
                for (var i = 0; i < created.Length; i++)
                    Assert.That(created[i].gameObject.scene, Is.EqualTo(_scene));
                RecordComponentNativeBytes(created, nativeGroup);

                var unload = SceneManager.UnloadSceneAsync(_scene);
                Assert.That(unload, Is.Not.Null);
                yield return unload;
                yield return null;

                Assert.That(_root == null, Is.True);
                AssertDestroyed(created);
                Measure.Custom(destroyedGroup, ObjectCount);
                _scene = default;
                _root = null;
            }
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (_scene.IsValid() && _scene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(_scene);
                if (unload != null)
                    yield return unload;
            }
            if (_root != null)
            {
                Object.Destroy(_root);
                yield return null;
            }
            _scene = default;
            _root = null;
        }

        private static void CreateComponents(Container container, LifetimeComponent[] instances)
        {
            container.Bind<LifetimeComponent>().FromNewComponentOnNewGameObject().AsTransient();
            for (var i = 0; i < instances.Length; i++)
                instances[i] = container.Resolve<LifetimeComponent>();
        }

        private static void RecordComponentNativeBytes(LifetimeComponent[] instances, SampleGroup group)
        {
            long bytes = 0;
            for (var i = 0; i < instances.Length; i++)
                bytes += Profiler.GetRuntimeMemorySizeLong(instances[i]);
            // This metric includes only these components, not their GameObjects or the entire native heap.
            // The API returns zero when the profiler is unavailable; do not report that as a footprint.
            if (bytes > 0)
                Measure.Custom(group, bytes);
        }

        private static void AssertDestroyed(LifetimeComponent[] instances)
        {
            for (var i = 0; i < instances.Length; i++)
            {
                Assert.That(instances[i] == null, Is.True, "A native component survived context teardown.");
                instances[i] = null;
            }
        }

        private static void RequirePlayMode()
        {
            if (!Application.isPlaying)
                Assert.Ignore("Deferred Destroy and scene unloading are measured in PlayMode.");
        }

        public sealed class LifetimeComponent : MonoBehaviour { }
    }
}
