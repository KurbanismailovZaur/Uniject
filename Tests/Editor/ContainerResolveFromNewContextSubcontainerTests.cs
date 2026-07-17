using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Components;
using Uniject.Contexts;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests
{
    public class ContainerResolveFromNewContextSubcontainerTests
    {
        private sealed class TransientService
        {
            public TransientService()
            {
            }
        }

        [Test]
        public void Resolve_ByNewContext_WhenScopeIsNotSpecified_CreatesBuiltContextForEveryResolve()
        {
            var contexts = new List<GameObjectContext>();
            var dependency = new Class();

            try
            {
                var container = new Container();
                container.BindInstance(dependency);
                container.Bind<InjectableScript>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<InjectableScript>()
                            .FromNewComponentOnNewGameObject()
                            .AsTransient();
                    });

                var first = container.Resolve<InjectableScript>();
                var second = container.Resolve<InjectableScript>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(2));
                Assert.That(contexts[1], Is.Not.SameAs(contexts[0]));
                Assert.That(first.Dependency, Is.SameAs(dependency));
                Assert.That(second.Dependency, Is.SameAs(dependency));
                Assert.That(first.transform.parent, Is.SameAs(contexts[0].transform));
                Assert.That(second.transform.parent, Is.SameAs(contexts[1].transform));

                foreach (var context in contexts)
                {
                    Assert.That(context.name, Is.EqualTo("GameObjectContext"));
                    AssertContextIsBuilt(context);
                }
            }
            finally
            {
                DestroyContexts(contexts);
            }
        }

        [Test]
        public void Resolve_ByNewContext_AsCached_ReusesContextButNotTransientContract()
        {
            var contexts = new List<GameObjectContext>();

            try
            {
                var container = new Container();
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<TransientService>().AsTransient();
                    })
                    .AsCached();

                var first = container.Resolve<TransientService>();
                var second = container.Resolve<TransientService>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(1));
                AssertContextIsBuilt(contexts[0]);
            }
            finally
            {
                DestroyContexts(contexts);
            }
        }

        [Test]
        public void Resolve_ByNewContext_WithNameAndUnderTransform_AsCached_AppliesConfigurationAndReusesContext()
        {
            var contexts = new List<GameObjectContext>();
            var inheritedParent = new GameObject("InheritedParent").transform;
            var explicitParent = new GameObject("ExplicitParent").transform;

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = inheritedParent
                };
                container.Bind<Script>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<Script>()
                            .FromNewComponentOnNewGameObject()
                            .AsTransient();
                    })
                    .WithGameObjectName("EnemyContext")
                    .UnderTransform(explicitParent)
                    .AsCached();

                var first = container.Resolve<Script>();
                var second = container.Resolve<Script>();
                var context = contexts[0];

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(1));
                Assert.That(context.name, Is.EqualTo("EnemyContext"));
                Assert.That(context.transform.parent, Is.SameAs(explicitParent));
                Assert.That(context.transform.parent, Is.Not.SameAs(inheritedParent));
                Assert.That(first.transform.parent, Is.SameAs(context.transform));
                Assert.That(second.transform.parent, Is.SameAs(context.transform));
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(explicitParent.gameObject);
                UnityEngine.Object.DestroyImmediate(inheritedParent.gameObject);
            }
        }

        [Test]
        public void Resolve_ByNewContext_WhenContainerHasParentTransform_UsesInheritedParent()
        {
            var contexts = new List<GameObjectContext>();
            var inheritedParent = new GameObject("InheritedParent").transform;

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = inheritedParent
                };
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<TransientService>().AsTransient();
                    })
                    .AsCached();

                container.Resolve<TransientService>();

                Assert.That(contexts[0].transform.parent, Is.SameAs(inheritedParent));
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(inheritedParent.gameObject);
            }
        }

        [Test]
        public void Resolve_ByNewContext_FromGameObjectContext_ParentsDynamicContextToOwnerContext()
        {
            var ownerObject = new GameObject("OwnerContext");
            var contexts = new List<GameObjectContext>();

            try
            {
                var ownerContext = ownerObject.AddComponent<GameObjectContext>();
                ownerContext.Initialize();
                ownerContext.Install();
                ownerContext.Container.Bind<Script>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<Script>()
                            .FromNewComponentOnNewGameObject()
                            .AsTransient();
                    })
                    .AsCached();
                ownerContext.Build();

                var resolved = ownerContext.Container.Resolve<Script>();
                var context = contexts[0];

                Assert.That(context.transform.parent, Is.SameAs(ownerContext.transform));
                Assert.That(resolved.transform.parent, Is.SameAs(context.transform));
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(ownerObject);
            }
        }

        [Test]
        public void Resolve_ByNewContext_FromSceneContext_MovesDynamicContextToOwnerScene()
        {
            var ownerScene = EditorSceneManager.NewPreviewScene();
            var ownerObject = new GameObject("OwnerContext");
            var contexts = new List<GameObjectContext>();

            try
            {
                SceneManager.MoveGameObjectToScene(ownerObject, ownerScene);

                var ownerContext = ownerObject.AddComponent<SceneContext>();
                ownerContext.Initialize();
                ownerContext.Install();
                ownerContext.Container.Bind<Script>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<Script>()
                            .FromNewComponentOnNewGameObject()
                            .AsTransient();
                    })
                    .AsCached();
                ownerContext.Build();

                var resolved = ownerContext.Container.Resolve<Script>();
                var context = contexts[0];

                Assert.That(context.transform.parent, Is.Null);
                Assert.That(context.gameObject.scene, Is.EqualTo(ownerScene));
                Assert.That(resolved.transform.parent, Is.SameAs(context.transform));
                Assert.That(resolved.gameObject.scene, Is.EqualTo(ownerScene));
            }
            finally
            {
                DestroyContexts(contexts);
                UnityEngine.Object.DestroyImmediate(ownerObject);

                if (ownerScene.IsValid() && ownerScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(ownerScene);
            }
        }

        [Test]
        public void Resolve_ByNewContext_WhenInstallMethodIsNull_ResolvesContainerFromEmptyContext()
        {
            GameObjectContext context = null;

            try
            {
                var container = new Container();
                container.Bind<object>().To<Container>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(null)
                    .AsCached();

                var resolved = (Container)container.Resolve<object>();
                context = (GameObjectContext)resolved.Context;

                Assert.That(resolved, Is.Not.SameAs(container));
                Assert.That(context.name, Is.EqualTo("GameObjectContext"));
                AssertContextIsBuilt(context);
            }
            finally
            {
                if (context != null)
                    UnityEngine.Object.DestroyImmediate(context.gameObject);
            }
        }

        [Test]
        public void Build_ByNewContextWithName_NonLazy_ReusesCreatedContext()
        {
            var contexts = new List<GameObjectContext>();

            try
            {
                var container = new Container();
                container.Bind<TransientService>()
                    .FromSubcontainerResolve()
                    .ByNewContextFromMethodOnNewGameObject(subcontainer =>
                    {
                        contexts.Add((GameObjectContext)subcontainer.Context);
                        subcontainer.Bind<TransientService>().AsTransient();
                    })
                    .WithGameObjectName("NonLazyContext")
                    .NonLazy();

                container.Build();
                var first = container.Resolve<TransientService>();
                var second = container.Resolve<TransientService>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(contexts, Has.Count.EqualTo(1));
                Assert.That(contexts[0].name, Is.EqualTo("NonLazyContext"));
                AssertContextIsBuilt(contexts[0]);
            }
            finally
            {
                DestroyContexts(contexts);
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
            Assert.That(context.GetComponents<TickableManager>(), Has.Length.EqualTo(1));
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
