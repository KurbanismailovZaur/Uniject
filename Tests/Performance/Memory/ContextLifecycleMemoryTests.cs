using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Installers;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.Memory
{
    public class ContextLifecycleMemoryTests
    {
        [Test, Performance]
        public void Initialize_WithChildren([Values(0, 16, 128)] int childCount,
            [Values(false, true)] bool sceneContext)
        {
            using var parent = new Container();
            Context context = null;
            AllocationMeasurement.Run("ContextInitialize", () => context.Initialize(parent),
                setUp: () => context = CreateTree(childCount, sceneContext),
                cleanUp: () => Destroy(context), iterations: 1);
        }

        [Test, Performance]
        public void Install_WithInstallers([Values(1, 16, 128)] int count,
            [Values(false, true)] bool siblingInstallers)
        {
            MemoryGameObjectContext context = null;
            AllocationMeasurement.Run("ContextInstallers", () => context.Install(), setUp: () =>
            {
                context = CreateGameObjectContext();
                var installers = new List<MonoInstaller>(count);
                for (var i = 0; i < count; i++)
                    installers.Add(context.gameObject.AddComponent<EmptyInstaller>());
                context.Configure(siblingInstallers: siblingInstallers, installers: installers);
                context.Initialize();
            }, cleanUp: () => Destroy(context), iterations: 1);
        }

        [Test, Performance]
        public void Install_Hierarchy_WithWarmStaticCollectionPool([Values(1, 16, 128)] int count,
            [Values(false, true)] bool withInjection)
        {
            MemoryGameObjectContext context = null;
            AllocationMeasurement.Run("ContextHierarchyScan", () => context.Install(), setUp: () =>
            {
                context = CreateHierarchy(count, withInjection);
                context.Configure(scanHierarchy: true);
                context.Initialize();
            }, cleanUp: () => Destroy(context), iterations: 1);
        }

        [Test, Performance]
        public void Build_WithQueuedTargets([Values(1, 16, 128)] int count,
            [Values(false, true)] bool withInjection)
        {
            MemoryGameObjectContext context = null;
            AllocationMeasurement.Run("ContextBuild", () => context.Build(), setUp: () =>
            {
                context = CreateHierarchy(count, withInjection);
                context.Configure(scanHierarchy: true);
                context.Initialize();
                context.Install();
            }, cleanUp: () => Destroy(context), iterations: 1);
        }

        [Test, Performance]
        public void Run_CompleteLifecycle_WithWarmReflection([Values(1, 16, 128)] int count)
        {
            MemoryGameObjectContext context = null;
            AllocationMeasurement.Run("ContextRun", () => context.Run(), setUp: () =>
            {
                context = CreateHierarchy(count, true);
                context.Configure(scanHierarchy: true);
            }, cleanUp: () => Destroy(context), iterations: 1);
        }

        [Test, Performance]
        public void Run_AlreadyRunContext()
        {
            var context = CreateGameObjectContext();
            try
            {
                context.Run();
                AllocationMeasurement.Run("ContextRunAgain", () => context.Run(), expectZero: true);
            }
            finally
            {
                Destroy(context);
            }
        }

        [Test, Performance]
        public void Destroy_ContextWithBuiltHierarchy([Values(1, 16, 128)] int count)
        {
            MemoryGameObjectContext context = null;
            DisposalProbe probe = null;
            AllocationMeasurement.Run("ContextDestroyImmediate", () => Object.DestroyImmediate(context.gameObject),
                setUp: () =>
                {
                    context = CreateHierarchy(count, true);
                    context.Configure(scanHierarchy: true);
                    context.Run();
                    probe = new DisposalProbe();
                    context.Container.BindInstance(probe).DisposeWithContainer();
                }, cleanUp: () =>
                {
                    Destroy(context);
                    Assert.That(probe.DisposeCount, Is.EqualTo(1), "OnDestroy must dispose the context container.");
                }, iterations: 1);
        }

        private static Context CreateTree(int childCount, bool sceneContext)
        {
            var root = new GameObject("Memory context tree");
            var children = new List<GameObjectContext>(childCount);
            Context context;
            if (sceneContext)
            {
                var scene = root.AddComponent<MemorySceneContext>();
                scene.Configure(children);
                context = scene;
            }
            else
            {
                var gameObjectContext = root.AddComponent<MemoryGameObjectContext>();
                gameObjectContext.Configure(children: children);
                context = gameObjectContext;
            }

            for (var i = 0; i < childCount; i++)
            {
                var child = CreateGameObjectContext();
                child.transform.SetParent(root.transform);
                children.Add(child);
            }

            return context;
        }

        private static MemoryGameObjectContext CreateHierarchy(int count, bool withInjection)
        {
            var context = CreateGameObjectContext();
            for (var i = 0; i < count; i++)
            {
                var child = new GameObject("Memory target");
                child.transform.SetParent(context.transform);
                child.AddComponent(withInjection ? typeof(InjectedTarget) : typeof(PlainTarget));
            }

            return context;
        }

        private static MemoryGameObjectContext CreateGameObjectContext()
        {
            var context = new GameObject("Memory context").AddComponent<MemoryGameObjectContext>();
            context.Configure();
            return context;
        }

        private static void Destroy(Context context)
        {
            if (context != null)
                Object.DestroyImmediate(context.gameObject);
        }

        public sealed class MemoryGameObjectContext : GameObjectContext
        {
            public void Configure(bool scanHierarchy = false, bool siblingInstallers = false,
                List<MonoInstaller> installers = null, List<GameObjectContext> children = null)
            {
#if UNITY_EDITOR
                runInEditMode = true;
#endif
                _injectInAllContextGameObjects = scanHierarchy;
                _useSiblingInstallers = siblingInstallers;
                _installers = installers ?? new List<MonoInstaller>();
                _gameObjectContexts = children ?? new List<GameObjectContext>();
            }
        }

        public sealed class MemorySceneContext : SceneContext
        {
            public void Configure(List<GameObjectContext> children)
            {
                enabled = false;
#if UNITY_EDITOR
                runInEditMode = true;
#endif
                _injectInAllContextGameObjects = false;
                _useSiblingInstallers = false;
                _gameObjectContexts = children;
            }
        }

        public sealed class EmptyInstaller : MonoInstaller
        {
            public override void Install(Container container) { }
        }

        public sealed class PlainTarget : MonoBehaviour { }

        private sealed class DisposalProbe : IDisposable
        {
            public int DisposeCount;
            public void Dispose() => DisposeCount++;
        }

        public sealed class InjectedTarget : MonoBehaviour
        {
            [Inject, Preserve]
            private void Construct(Container container) { }
        }
    }
}
