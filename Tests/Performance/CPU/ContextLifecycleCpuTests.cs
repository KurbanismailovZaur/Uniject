using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Components;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class ContextLifecycleCpuTests
    {
        private const string SceneLoaderSceneName = "SceneLoaderRequested";
        private const string SceneLoaderScenePath =
            "Packages/com.codomaster.uniject/Tests/Runtime/Fixtures/Scenes/SceneLoaderRequested.unity";

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Initialize_WithoutChildren(bool sceneContext)
        {
            using var parent = new Container();
            Context context = null;

            Measure.Method(() => context.Initialize(parent))
                .SetUp(() => context = CreateContext(sceneContext))
                .CleanUp(() => Object.DestroyImmediate(context.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Initialize", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [Performance]
        public void Initialize_WithExplicitChildContexts(int childCount, bool sceneContext)
        {
            using var parent = new Container();
            Context context = null;

            Measure.Method(() => context.Initialize(parent))
                .SetUp(() => context = CreateContextTree(childCount + 1, sceneContext))
                .CleanUp(() => Object.DestroyImmediate(context.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Initialize", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [Performance]
        public void Install_Installers(int installerCount, bool siblingInstallers)
        {
            GameObject root = null;
            TestGameObjectContext context = null;

            Measure.Method(() => context.Install())
                .SetUp(() =>
                {
                    root = new GameObject("Context");
                    context = root.AddComponent<TestGameObjectContext>();
                    var installers = new List<MonoInstaller>(installerCount);
                    for (var i = 0; i < installerCount; i++)
                        installers.Add(root.AddComponent<EmptyInstaller>());

                    context.Configure(siblingInstallers: siblingInstallers, installers: installers);
                    context.Initialize();
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1000, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [TestCase(1000, true)]
        [Performance]
        public void Install_GameObjectHierarchy(int objectCount, bool withInjection)
        {
            GameObject root = null;
            TestGameObjectContext context = null;
            var targetType = withInjection ? typeof(InjectedTarget) : typeof(PlainTarget);

            Measure.Method(() => context.Install())
                .SetUp(() =>
                {
                    root = new GameObject("Context");
                    context = root.AddComponent<TestGameObjectContext>();
                    root.AddComponent(targetType);
                    for (var i = 1; i < objectCount; i++)
                    {
                        var child = new GameObject("Target");
                        child.transform.SetParent(root.transform);
                        child.AddComponent(targetType);
                    }

                    context.Configure(scanHierarchy: true);
                    context.Initialize();
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Install_GameObjectHierarchy_SkipsNestedContextSubtree(int skippedObjectCount)
        {
            GameObject root = null;
            TestGameObjectContext context = null;

            Measure.Method(() => context.Install())
                .SetUp(() =>
                {
                    root = new GameObject("Context");
                    context = root.AddComponent<TestGameObjectContext>();
                    root.AddComponent<InjectedTarget>();
                    var nestedRoot = new GameObject("NestedContext");
                    nestedRoot.transform.SetParent(root.transform);
                    nestedRoot.AddComponent<TestGameObjectContext>().Configure(scanHierarchy: true);
                    nestedRoot.AddComponent<InjectedTarget>();
                    for (var i = 1; i < skippedObjectCount; i++)
                    {
                        var child = new GameObject("SkippedTarget");
                        child.transform.SetParent(nestedRoot.transform);
                        child.AddComponent<InjectedTarget>();
                    }

                    context.Configure(scanHierarchy: true);
                    context.Initialize();
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1, false)]
        [TestCase(10, false)]
        [TestCase(100, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        [TestCase(100, true)]
        [Performance]
        public void Install_ExplicitTargets(int targetCount, bool wrapped)
        {
            GameObject root = null;
            TestGameObjectContext context = null;

            Measure.Method(() => context.Install())
                .SetUp(() =>
                {
                    root = new GameObject("Context");
                    context = root.AddComponent<TestGameObjectContext>();
                    var targets = new List<MonoBehaviour>(targetCount);
                    for (var i = 0; i < targetCount; i++)
                        targets.Add(root.AddComponent<InjectedTarget>());

                    if (wrapped)
                    {
                        var wrapper = root.AddComponent<InjectTargets>();
                        typeof(InjectTargets)
                            .GetField("<Targets>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                            .SetValue(wrapper, targets.ToArray());
                        targets = new List<MonoBehaviour> { wrapper };
                    }

                    context.Configure(targets: targets);
                    context.Initialize();
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Build_WithNonLazyBindingsTargetsAndEntryPoint(bool sceneContext)
        {
            Context context = null;

            Measure.Method(() => context.Build())
                .SetUp(() =>
                {
                    context = CreateContext(sceneContext, withWorkload: true);
                    context.Initialize();
                    context.Install();
                })
                .CleanUp(() => Object.DestroyImmediate(context.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void Build_InitializedAndInstalledTree(int contextCount)
        {
            Context context = null;

            Measure.Method(() => context.Build())
                .SetUp(() =>
                {
                    context = CreateContextTree(contextCount, false, withWorkload: true);
                    context.Initialize();
                    context.Install();
                })
                .CleanUp(() => Object.DestroyImmediate(context.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Build", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Run_NewContextWithInstallerAndTarget(bool sceneContext)
        {
            Context context = null;

            Measure.Method(() => context.Run())
                .SetUp(() => context = CreateContext(sceneContext, withWorkload: true))
                .CleanUp(() => Object.DestroyImmediate(context.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Run", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false, "Initialize")]
        [TestCase(true, "Initialize")]
        [TestCase(false, "Install")]
        [TestCase(true, "Install")]
        [TestCase(false, "Build")]
        [TestCase(true, "Build")]
        [TestCase(false, "Run")]
        [TestCase(true, "Run")]
        [Performance]
        public void Lifecycle_CompletedStage(bool sceneContext, string stage)
        {
            var context = CreateContext(sceneContext, withWorkload: true);
            context.Run();
            Action action = stage switch
            {
                "Initialize" => () => context.Initialize(),
                "Install" => context.Install,
                "Build" => context.Build,
                _ => context.Run
            };

            Measure.Method(action)
                .SampleGroup(new SampleGroup(stage, SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(context.gameObject);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        [PrebuildSetup("Uniject.Tests.SceneLoaderTests")]
        [PostBuildCleanup("Uniject.Tests.SceneLoaderTests")]
        public async Task SceneContext_Install_ScanRootObjects(int rootCount)
        {
            await SceneManager.LoadSceneAsync(SceneLoaderSceneName, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByName(SceneLoaderSceneName);
            var contextRoot = scene.GetRootGameObjects()[0];
            Object.DestroyImmediate(contextRoot.GetComponent<SceneContext>());
            for (var i = 1; i < rootCount; i++)
                SceneManager.MoveGameObjectToScene(new GameObject("Root"), scene);
            TestSceneContext context = null;

            Measure.Method(() => context.Install())
                .SetUp(() =>
                {
                    context = contextRoot.AddComponent<TestSceneContext>();
                    context.Configure(scanHierarchy: true);
                    context.Initialize();
                })
                .CleanUp(() => Object.DestroyImmediate(context))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();

            await SceneManager.UnloadSceneAsync(scene);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        [PrebuildSetup("Uniject.Tests.SceneLoaderTests")]
        [PostBuildCleanup("Uniject.Tests.SceneLoaderTests")]
        public async Task SceneLoader_LoadSceneAdditiveAsync_SynchronousCall(bool byBuildIndex)
        {
            using var container = new Container();
            var loader = new SceneLoader(container);
            var sceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(SceneLoaderScenePath);
            Func<Awaitable> load = byBuildIndex
                ? () => loader.LoadSceneAdditiveAsync(sceneBuildIndex)
                : () => loader.LoadSceneAdditiveAsync(SceneLoaderSceneName);
            var sampleGroup = new SampleGroup("LoadSceneAdditiveAsync.Start", SampleUnit.Nanosecond);

            for (var iteration = 0; iteration < 35; iteration++)
            {
                Awaitable operation;
                if (iteration < 5)
                    operation = load();
                else
                    using (Measure.Scope(sampleGroup))
                        operation = load();

                await operation;
                await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(SceneLoaderSceneName));
            }
        }

        private static Context CreateContext(bool sceneContext, bool withWorkload = false,
            List<GameObjectContext> children = null)
        {
            var gameObject = new GameObject("Context");
            var installers = new List<MonoInstaller>();
            var targets = new List<MonoBehaviour>();

            if (withWorkload)
            {
                installers.Add(gameObject.AddComponent<WorkloadInstaller>());
                targets.Add(gameObject.AddComponent<WorkloadTarget>());
            }

            if (sceneContext)
            {
                var context = gameObject.AddComponent<TestSceneContext>();
                context.Configure(installers: installers, targets: targets, children: children);
                return context;
            }

            var gameObjectContext = gameObject.AddComponent<TestGameObjectContext>();
            gameObjectContext.Configure(installers: installers, targets: targets, children: children);
            return gameObjectContext;
        }

        private static Context CreateContextTree(int count, bool sceneContext, bool withWorkload = false)
        {
            var contexts = new Context[count];
            var children = new List<GameObjectContext>[count];

            for (var i = 0; i < count; i++)
            {
                children[i] = new List<GameObjectContext>();
                contexts[i] = CreateContext(i == 0 && sceneContext, withWorkload, children[i]);
            }

            for (var i = 1; i < count; i++)
            {
                var parentIndex = (i - 1) / 2;
                children[parentIndex].Add((GameObjectContext)contexts[i]);
                contexts[i].transform.SetParent(contexts[parentIndex].transform);
            }

            return contexts[0];
        }

        public sealed class EmptyInstaller : MonoInstaller
        {
            public override void Install(Container container) { }
        }

        public sealed class PlainTarget : MonoBehaviour { }

        public sealed class InjectedTarget : MonoBehaviour
        {
            [Inject]
            private void Construct(Container container) { }
        }

        public sealed class TestGameObjectContext : GameObjectContext
        {
            public void Configure(bool siblingInstallers = false, bool scanHierarchy = false,
                List<MonoInstaller> installers = null, List<MonoBehaviour> targets = null,
                List<GameObjectContext> children = null)
            {
                _useSiblingInstallers = siblingInstallers;
                _injectInAllContextGameObjects = scanHierarchy;
                _installers = installers ?? new List<MonoInstaller>();
                _injectTargets = targets ?? new List<MonoBehaviour>();
                _gameObjectContexts = children ?? new List<GameObjectContext>();
            }
        }

        public sealed class TestSceneContext : SceneContext
        {
            public void Configure(bool siblingInstallers = false, bool scanHierarchy = false,
                List<MonoInstaller> installers = null, List<MonoBehaviour> targets = null,
                List<GameObjectContext> children = null)
            {
                enabled = false;
                _useSiblingInstallers = siblingInstallers;
                _injectInAllContextGameObjects = scanHierarchy;
                _installers = installers ?? new List<MonoInstaller>();
                _injectTargets = targets ?? new List<MonoBehaviour>();
                _gameObjectContexts = children ?? new List<GameObjectContext>();
            }
        }

        public sealed class WorkloadInstaller : MonoInstaller
        {
            public override void Install(Container container)
            {
                container.Bind<Dependency>().AsCached().NonLazy();
                container.Bind<Service>().AsCached().NonLazy();
                container.Bind<EntryPoint>().AsEntryPoint();
            }
        }

        public sealed class WorkloadTarget : MonoBehaviour
        {
            [Inject]
            private void Construct(Service service) { }
        }

        private sealed class Dependency
        {
            [Preserve]
            public Dependency() { }
        }

        private sealed class Service
        {
            [Preserve]
            public Service(Dependency dependency) { }
        }

        private sealed class EntryPoint : IEntryPoint
        {
            [Preserve]
            public EntryPoint(Service service) { }

            public void Run() { }
        }
    }
}
