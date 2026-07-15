using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class ContextLifecycleTestInstaller : MonoInstaller
    {
        public string EventName { get; set; }
        public Action<Container> InstallAction { get; set; }
        public int InstallCallsCount { get; private set; }

        public override void Install(Container container)
        {
            InstallCallsCount++;
            ContextLifecycleTestEvents.Items.Add(EventName);
            InstallAction?.Invoke(container);
        }
    }

    public static class ContextLifecycleTestEvents
    {
        public static readonly List<string> Items = new();
    }

    public sealed class ContextParentDependency
    {
    }

    public sealed class ParentContextNonLazyProbe
    {
        public ParentContextNonLazyProbe() => ContextLifecycleTestEvents.Items.Add("parent build");
    }

    public sealed class ChildContextNonLazyProbe
    {
        public ChildContextNonLazyProbe() => ContextLifecycleTestEvents.Items.Add("child build");
    }

    public sealed class ContextInjectTarget : MonoBehaviour
    {
        public ContextParentDependency Dependency { get; private set; }

        [Inject]
        public void Construct(ContextParentDependency dependency) => Dependency = dependency;
    }

    public class ContextLifecycleTests
    {
        [SetUp]
        public void SetUp() => ContextLifecycleTestEvents.Items.Clear();

        [TearDown]
        public void TearDown() => ContextLifecycleTestEvents.Items.Clear();

        [Test]
        public void Run_WithConfiguredGameObjectContext_RunsLifecycleInHierarchyOrderOnce()
        {
            var sceneContextObject = new GameObject("SceneContext");
            var gameObjectContextObject = new GameObject("GameObjectContext");

            try
            {
                var sceneContext = sceneContextObject.AddComponent<SceneContext>();
                var gameObjectContext = gameObjectContextObject.AddComponent<GameObjectContext>();
                var parentInstaller = sceneContextObject.AddComponent<ContextLifecycleTestInstaller>();
                var childInstaller = gameObjectContextObject.AddComponent<ContextLifecycleTestInstaller>();
                var injectTarget = gameObjectContextObject.AddComponent<ContextInjectTarget>();
                var dependency = new ContextParentDependency();

                parentInstaller.EventName = "parent install";
                parentInstaller.InstallAction = container =>
                {
                    container.BindInstance(dependency);
                    container.Bind<ParentContextNonLazyProbe>().AsTransient().NonLazy();
                };

                childInstaller.EventName = "child install";
                childInstaller.InstallAction = container =>
                    container.Bind<ChildContextNonLazyProbe>().AsTransient().NonLazy();

                ContextTestUtility.Configure(
                    sceneContext,
                    installers: new[] { parentInstaller },
                    gameObjectContexts: new[] { gameObjectContext });
                ContextTestUtility.Configure(
                    gameObjectContext,
                    installers: new[] { childInstaller },
                    injectTargets: new[] { injectTarget });

                sceneContext.Run();
                sceneContext.Run();

                Assert.That(sceneContext.IsInitialized, Is.True);
                Assert.That(sceneContext.IsInstalled, Is.True);
                Assert.That(sceneContext.IsBuilded, Is.True);
                Assert.That(gameObjectContext.IsInitialized, Is.True);
                Assert.That(gameObjectContext.IsInstalled, Is.True);
                Assert.That(gameObjectContext.IsBuilded, Is.True);
                Assert.That(parentInstaller.InstallCallsCount, Is.EqualTo(1));
                Assert.That(childInstaller.InstallCallsCount, Is.EqualTo(1));
                Assert.That(
                    ContextLifecycleTestEvents.Items,
                    Is.EqualTo(new[] { "parent install", "child install", "parent build", "child build" }));
                Assert.That(injectTarget.Dependency, Is.SameAs(dependency));
                Assert.That(gameObjectContext.Container.Resolve<ContextParentDependency>(), Is.SameAs(dependency));
                Assert.That(sceneContext.Container.Context, Is.SameAs(sceneContext));
                Assert.That(gameObjectContext.Container.Context, Is.SameAs(gameObjectContext));
                Assert.That(sceneContext.Container.GetNearestContext(), Is.SameAs(sceneContext));
                Assert.That(gameObjectContext.Container.GetNearestContext(), Is.SameAs(gameObjectContext));
                Assert.That(sceneContextObject.GetComponents<TickableManager>(), Has.Length.EqualTo(1));
                Assert.That(gameObjectContextObject.GetComponents<TickableManager>(), Has.Length.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObjectContextObject);
                UnityEngine.Object.DestroyImmediate(sceneContextObject);
            }
        }

        [Test]
        public void Initialize_WithParentContainerBeforeRun_PreservesParentAndAdditionalBindings()
        {
            var sceneContextObject = new GameObject("SceneContext");

            try
            {
                var parentContainer = new Container();
                var parentDependency = new ContextParentDependency();
                var additionalDependency = new ChildContextNonLazyProbe();
                parentContainer.BindInstance(parentDependency);

                var sceneContext = sceneContextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(sceneContext);

                sceneContext.Initialize(parentContainer);
                sceneContext.Container.BindInstance(additionalDependency);
                sceneContext.Run();

                Assert.That(sceneContext.Container.Resolve<ContextParentDependency>(), Is.SameAs(parentDependency));
                Assert.That(sceneContext.Container.Resolve<ChildContextNonLazyProbe>(), Is.SameAs(additionalDependency));
                Assert.That(sceneContext.IsBuilded, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sceneContextObject);
            }
        }

        [Test]
        public void Start_RunsSceneContextLifecycle()
        {
            var sceneContextObject = new GameObject("SceneContext");

            try
            {
                var sceneContext = sceneContextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(sceneContext);
                var startMethod = typeof(SceneContext).GetMethod(
                    "Start",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(startMethod, Is.Not.Null);
                startMethod.Invoke(sceneContext, null);

                Assert.That(sceneContext.IsInitialized, Is.True);
                Assert.That(sceneContext.IsInstalled, Is.True);
                Assert.That(sceneContext.IsBuilded, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sceneContextObject);
            }
        }
    }
}
