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
    public class ContainerResolveFromNewContextPrefabSubcontainerTests
    {
        private sealed class TransientService
        {
            public TransientService()
            {
            }
        }

        private sealed class CountingPrefabInstaller : IInstaller
        {
            public int InstallCallsCount { get; private set; }
            public List<GameObjectContext> Contexts { get; } = new();

            public CountingPrefabInstaller()
            {
            }

            public void Install(Container container)
            {
                InstallCallsCount++;
                Contexts.Add((GameObjectContext)container.Context);
                container.Bind<TransientService>().AsTransient();
            }
        }

        private sealed class GenericCountingPrefabInstaller : IInstaller
        {
            public static int InstancesCount { get; private set; }
            public static int InstallCallsCount { get; private set; }
            public static List<GameObjectContext> Contexts { get; } = new();

            public GenericCountingPrefabInstaller()
            {
                InstancesCount++;
            }

            public void Install(Container container)
            {
                InstallCallsCount++;
                Contexts.Add((GameObjectContext)container.Context);
                container.Bind<TransientService>().AsTransient();
            }

            public static void Reset()
            {
                InstancesCount = 0;
                InstallCallsCount = 0;
                Contexts.Clear();
            }
        }

        [Test]
        public void ByNewContextFromMethodOnNewPrefab_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewPrefab(null, _ => { }),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ByNewContextFromInstallerOnNewPrefab_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            var installer = new CountingPrefabInstaller();

            Assert.That(
                () => container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromInstallerOnNewPrefab(null, installer),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ByNewContextFromInstallerOnNewPrefabGeneric_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            GenericCountingPrefabInstaller.Reset();

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<TransientService>()
                        .FromSubcontainerResolve()
                        .ByNewContextFromInstallerOnNewPrefab<GenericCountingPrefabInstaller>(null),
                    Throws.TypeOf<ArgumentNullException>());
            }
            finally
            {
                GenericCountingPrefabInstaller.Reset();
            }
        }

        [Test]
        public void ByNewContextFromInstallerOnNewPrefab_WhenInstallerIsNull_ThrowsArgumentNullException()
        {
            var prefab = new GameObject("ContextPrefab");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<TransientService>()
                        .FromSubcontainerResolve()
                        .ByNewContextFromInstallerOnNewPrefab<CountingPrefabInstaller>(prefab, null),
                    Throws.TypeOf<ArgumentNullException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByNewContextFromMethodOnNewPrefab_WhenInstallMethodIsNull_BuildsEmptyContextOnClone()
        {
            var prefab = new GameObject("ContextPrefab");
            var prefabScript = prefab.AddComponent<Script>();
            GameObjectContext context = null;

            try
            {
                var container = new Container();
                container.Bind<object>().To<Container>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewPrefab(prefab, null)
                    .AsCached();

                var resolved = (Container)container.Resolve<object>();
                context = (GameObjectContext)resolved.Context;
                var clonedScript = context.GetComponent<Script>();

                Assert.That(resolved, Is.Not.SameAs(container));
                Assert.That(context.gameObject, Is.Not.SameAs(prefab));
                Assert.That(context.name, Is.EqualTo("GameObjectContext"));
                Assert.That(clonedScript, Is.Not.Null);
                Assert.That(clonedScript, Is.Not.SameAs(prefabScript));
                Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Null);
                Assert.That(prefab.name, Is.EqualTo("ContextPrefab"));
                AssertContextIsBuilt(context);
            }
            finally
            {
                if (context != null)
                    UnityEngine.Object.DestroyImmediate(context.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByNewContextFromMethodOnNewPrefab_WhenScopeIsNotSpecified_ClonesPrefabForEveryResolve()
        {
            var prefab = new GameObject("ContextPrefab");
            var prefabScript = prefab.AddComponent<Script>();
            var child = new GameObject("PrefabChild");
            child.transform.SetParent(prefab.transform);
            var contexts = new List<GameObjectContext>();

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewPrefab(prefab, subcontainer =>
                    {
                        var context = (GameObjectContext)subcontainer.Context;
                        contexts.Add(context);
                        subcontainer.BindInstance(context.GetComponent<Script>());
                    });

                var first = container.Resolve<Script>();
                var second = container.Resolve<Script>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(2));
                Assert.That(contexts[1], Is.Not.SameAs(contexts[0]));
                Assert.That(first.gameObject, Is.SameAs(contexts[0].gameObject));
                Assert.That(second.gameObject, Is.SameAs(contexts[1].gameObject));
                Assert.That(first, Is.Not.SameAs(prefabScript));
                Assert.That(second, Is.Not.SameAs(prefabScript));

                foreach (var context in contexts)
                {
                    Assert.That(context.gameObject, Is.Not.SameAs(prefab));
                    Assert.That(context.name, Is.EqualTo("GameObjectContext"));
                    Assert.That(context.transform.Find("PrefabChild"), Is.Not.Null);
                    AssertContextIsBuilt(context);
                }

                Assert.That(prefab.GetComponent<GameObjectContext>(), Is.Null);
                Assert.That(prefab.transform.Find("PrefabChild"), Is.SameAs(child.transform));
                Assert.That(prefab.name, Is.EqualTo("ContextPrefab"));
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByNewContextFromMethodOnNewPrefab_WithConfigurationAsCached_ReusesConfiguredContext()
        {
            var prefab = new GameObject("ContextPrefab");
            var inheritedParent = new GameObject("InheritedParent").transform;
            var explicitParent = new GameObject("ExplicitParent").transform;
            var contexts = new List<GameObjectContext>();

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = inheritedParent
                };
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewPrefab(prefab, subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<TransientService>().AsTransient();
                    })
                    .WithGameObjectName("ConfiguredContext")
                    .UnderTransform(explicitParent)
                    .AsCached();

                var first = container.Resolve<TransientService>();
                var second = container.Resolve<TransientService>();
                var context = contexts[0];

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(1));
                Assert.That(context.name, Is.EqualTo("ConfiguredContext"));
                Assert.That(context.transform.parent, Is.SameAs(explicitParent));
                Assert.That(context.transform.parent, Is.Not.SameAs(inheritedParent));
                Assert.That(prefab.name, Is.EqualTo("ContextPrefab"));
                Assert.That(prefab.transform.parent, Is.Null);
                AssertContextIsBuilt(context);
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(prefab);
                UnityEngine.Object.DestroyImmediate(explicitParent.gameObject);
                UnityEngine.Object.DestroyImmediate(inheritedParent.gameObject);
            }
        }

        [Test]
        public void Resolve_ByNewContextFromInstallerOnNewPrefab_AsCached_UsesInstallerOnce()
        {
            var prefab = new GameObject("ContextPrefab");
            var parent = new GameObject("Parent").transform;
            var installer = new CountingPrefabInstaller();

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromInstallerOnNewPrefab(prefab, installer)
                    .WithGameObjectName("InstallerContext")
                    .AsCached();

                var first = container.Resolve<TransientService>();
                var second = container.Resolve<TransientService>();
                var context = installer.Contexts[0];

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(installer.InstallCallsCount, Is.EqualTo(1));
                Assert.That(installer.Contexts, Has.Count.EqualTo(1));
                Assert.That(context.name, Is.EqualTo("InstallerContext"));
                Assert.That(context.transform.parent, Is.SameAs(parent));
                Assert.That(prefab.transform.parent, Is.Null);
                AssertContextIsBuilt(context);
            }
            finally
            {
                DestroyContexts(installer.Contexts);
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_ByNewContextFromInstallerOnNewPrefabGeneric_WhenScopeIsNotSpecified_CreatesInstallerOnceAndInstallsEveryContext()
        {
            var prefab = new GameObject("ContextPrefab");
            GenericCountingPrefabInstaller.Reset();

            try
            {
                var container = new Container();
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromInstallerOnNewPrefab<GenericCountingPrefabInstaller>(prefab);

                var first = container.Resolve<TransientService>();
                var second = container.Resolve<TransientService>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(GenericCountingPrefabInstaller.InstancesCount, Is.EqualTo(1));
                Assert.That(GenericCountingPrefabInstaller.InstallCallsCount, Is.EqualTo(2));
                Assert.That(GenericCountingPrefabInstaller.Contexts, Has.Count.EqualTo(2));
                Assert.That(GenericCountingPrefabInstaller.Contexts[1],
                    Is.Not.SameAs(GenericCountingPrefabInstaller.Contexts[0]));

                foreach (var context in GenericCountingPrefabInstaller.Contexts)
                {
                    Assert.That(context.gameObject, Is.Not.SameAs(prefab));
                    AssertContextIsBuilt(context);
                }
            }
            finally
            {
                DestroyContexts(GenericCountingPrefabInstaller.Contexts);
                GenericCountingPrefabInstaller.Reset();
                UnityEngine.Object.DestroyImmediate(prefab);
            }
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

        private static void DestroyContexts(IReadOnlyList<GameObjectContext> contexts)
        {
            for (var i = contexts.Count - 1; i >= 0; i--)
            {
                if (contexts[i] != null)
                    UnityEngine.Object.DestroyImmediate(contexts[i].gameObject);
            }
        }
    }
}
