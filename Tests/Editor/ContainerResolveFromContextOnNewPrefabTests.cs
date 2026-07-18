using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Components;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class ContextOnNewPrefabService
    {
        public ContextOnNewPrefabService()
        {
        }
    }

    public sealed class ContextOnNewPrefabInstaller : MonoInstaller
    {
        public static List<Container> InstalledContainers { get; } = new();

        public override void Install(Container container)
        {
            InstalledContainers.Add(container);
            container.Bind<ContextOnNewPrefabService>().AsTransient();
        }

        public static void ClearInstalledContainers() => InstalledContainers.Clear();
    }

    public class ContainerResolveFromContextOnNewPrefabTests
    {
        [Test]
        public void ByContextOnNewPrefab_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<ContextOnNewPrefabService>()
                    .FromSubcontainerResolve()
                    .ByContextOnNewPrefab(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_ByContextOnNewPrefab_WhenRootContextIsMissing_ThrowsArgumentException()
        {
            var prefab = new GameObject("PrefabWithoutContext");

            try
            {
                var container = new Container();
                container.Bind<ContextOnNewPrefabService>()
                    .FromSubcontainerResolve()
                    .ByContextOnNewPrefab(prefab)
                    .WithGameObjectName("MissingRootContextClone")
                    .AsCached();

                Assert.That(
                    () => container.Resolve<ContextOnNewPrefabService>(),
                    Throws.TypeOf<ArgumentException>());
                Assert.That(prefab.name, Is.EqualTo("PrefabWithoutContext"));
            }
            finally
            {
                var clone = GameObject.Find("MissingRootContextClone");

                if (clone != null)
                    UnityEngine.Object.DestroyImmediate(clone);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByContextOnNewPrefab_WhenScopeIsNotSpecified_ClonesAndRunsConfiguredContextForEveryResolve()
        {
            var prefab = new GameObject("ContextPrefab");
            var rootContext = prefab.AddComponent<GameObjectContext>();
            var installer = prefab.AddComponent<ContextOnNewPrefabInstaller>();
            var injectTarget = prefab.AddComponent<InjectableScript>();
            var childObject = new GameObject("ChildContext");
            childObject.transform.SetParent(prefab.transform);
            var childContext = childObject.AddComponent<GameObjectContext>();
            var dependency = new Class();

            ContextTestUtility.Configure(childContext);
            ContextTestUtility.Configure(
                rootContext,
                installers: new[] { installer },
                injectTargets: new[] { injectTarget },
                gameObjectContexts: new[] { childContext });
            ContextOnNewPrefabInstaller.ClearInstalledContainers();

            try
            {
                var container = new Container();
                container.BindInstance(dependency);
                container.Bind<ContextOnNewPrefabService>()
                    .FromSubcontainerResolve()
                    .ByContextOnNewPrefab(prefab);

                var first = container.Resolve<ContextOnNewPrefabService>();
                var second = container.Resolve<ContextOnNewPrefabService>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(ContextOnNewPrefabInstaller.InstalledContainers, Has.Count.EqualTo(2));

                foreach (var installedContainer in ContextOnNewPrefabInstaller.InstalledContainers)
                {
                    var clonedContext = (GameObjectContext)installedContainer.Context;
                    var clonedInjectTarget = clonedContext.GetComponent<InjectableScript>();
                    var clonedInstaller = clonedContext.GetComponent<ContextOnNewPrefabInstaller>();
                    var clonedChildContext = clonedContext.transform
                        .Find("ChildContext")
                        .GetComponent<GameObjectContext>();

                    Assert.That(clonedContext.gameObject, Is.Not.SameAs(prefab));
                    Assert.That(clonedContext.name, Is.EqualTo("GameObjectContext"));
                    Assert.That(clonedContext.GetComponents<GameObjectContext>(), Has.Length.EqualTo(1));
                    Assert.That(clonedInstaller, Is.Not.SameAs(installer));
                    Assert.That(clonedInjectTarget, Is.Not.SameAs(injectTarget));
                    Assert.That(clonedInjectTarget.Dependency, Is.SameAs(dependency));
                    Assert.That(clonedChildContext, Is.Not.SameAs(childContext));
                    Assert.That(clonedChildContext.Container.Resolve<Class>(), Is.SameAs(dependency));
                    AssertContextIsBuilt(clonedContext);
                    AssertContextIsBuilt(clonedChildContext);
                }

                Assert.That(rootContext.IsInitialized, Is.False);
                Assert.That(childContext.IsInitialized, Is.False);
                Assert.That(injectTarget.Dependency, Is.Null);
                Assert.That(prefab.GetComponents<TickableManager>(), Is.Empty);
                Assert.That(prefab.name, Is.EqualTo("ContextPrefab"));
            }
            finally
            {
                DestroyInstalledContexts();
                ContextOnNewPrefabInstaller.ClearInstalledContainers();
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByContextOnNewPrefab_WithConfigurationAsCached_ReusesConfiguredContext()
        {
            var prefab = CreatePrefabWithInstaller(out var prefabContext);
            var inheritedParent = new GameObject("InheritedParent").transform;
            var explicitParent = new GameObject("ExplicitParent").transform;
            ContextOnNewPrefabInstaller.ClearInstalledContainers();

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = inheritedParent
                };
                container.Bind<ContextOnNewPrefabService>()
                    .FromSubcontainerResolve()
                    .ByContextOnNewPrefab(prefab)
                    .WithGameObjectName("ConfiguredContext")
                    .UnderTransform(explicitParent)
                    .AsCached();

                var first = container.Resolve<ContextOnNewPrefabService>();
                var second = container.Resolve<ContextOnNewPrefabService>();
                var context = (GameObjectContext)ContextOnNewPrefabInstaller.InstalledContainers[0].Context;

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(ContextOnNewPrefabInstaller.InstalledContainers, Has.Count.EqualTo(1));
                Assert.That(context.name, Is.EqualTo("ConfiguredContext"));
                Assert.That(context.transform.parent, Is.SameAs(explicitParent));
                Assert.That(context.transform.parent, Is.Not.SameAs(inheritedParent));
                Assert.That(prefabContext.IsInitialized, Is.False);
                Assert.That(prefab.name, Is.EqualTo("ContextPrefab"));
                Assert.That(prefab.transform.parent, Is.Null);
                AssertContextIsBuilt(context);
            }
            finally
            {
                DestroyInstalledContexts();
                ContextOnNewPrefabInstaller.ClearInstalledContainers();
                UnityEngine.Object.DestroyImmediate(prefab);
                UnityEngine.Object.DestroyImmediate(explicitParent.gameObject);
                UnityEngine.Object.DestroyImmediate(inheritedParent.gameObject);
            }
        }

        [Test]
        public void Build_ByContextOnNewPrefab_NonLazyCreatesAndReusesContext()
        {
            var prefab = CreatePrefabWithInstaller(out var prefabContext);
            ContextOnNewPrefabInstaller.ClearInstalledContainers();

            try
            {
                var container = new Container();
                container.Bind<ContextOnNewPrefabService>()
                    .FromSubcontainerResolve()
                    .ByContextOnNewPrefab(prefab)
                    .WithGameObjectName("NonLazyContext")
                    .NonLazy();

                Assert.That(ContextOnNewPrefabInstaller.InstalledContainers, Is.Empty);

                container.Build();

                Assert.That(ContextOnNewPrefabInstaller.InstalledContainers, Has.Count.EqualTo(1));

                var first = container.Resolve<ContextOnNewPrefabService>();
                var second = container.Resolve<ContextOnNewPrefabService>();
                var context = (GameObjectContext)ContextOnNewPrefabInstaller.InstalledContainers[0].Context;

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(ContextOnNewPrefabInstaller.InstalledContainers, Has.Count.EqualTo(1));
                Assert.That(context.name, Is.EqualTo("NonLazyContext"));
                Assert.That(prefabContext.IsInitialized, Is.False);
                AssertContextIsBuilt(context);
            }
            finally
            {
                DestroyInstalledContexts();
                ContextOnNewPrefabInstaller.ClearInstalledContainers();
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        private static GameObject CreatePrefabWithInstaller(out GameObjectContext context)
        {
            var prefab = new GameObject("ContextPrefab");
            context = prefab.AddComponent<GameObjectContext>();
            var installer = prefab.AddComponent<ContextOnNewPrefabInstaller>();
            ContextTestUtility.Configure(context, installers: new[] { installer });
            return prefab;
        }

        private static void AssertContextIsBuilt(GameObjectContext context)
        {
            Assert.That(context, Is.Not.Null);
            Assert.That(context.IsInitialized, Is.True);
            Assert.That(context.IsInstalled, Is.True);
            Assert.That(context.IsBuilded, Is.True);
            Assert.That(context.Container.IsBuilded, Is.True);
            Assert.That(context.Container.Context, Is.SameAs(context));
            Assert.That(context.GetComponents<TickableManager>(), Is.Empty);
        }

        private static void DestroyInstalledContexts()
        {
            for (var i = ContextOnNewPrefabInstaller.InstalledContainers.Count - 1; i >= 0; i--)
            {
                var context = ContextOnNewPrefabInstaller.InstalledContainers[i].Context as GameObjectContext;

                if (context != null)
                    UnityEngine.Object.DestroyImmediate(context.gameObject);
            }
        }
    }
}
