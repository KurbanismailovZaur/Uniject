using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Tests.Fixtures;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests
{
    public class ContainerResolveGameObjectConfigurationTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WithGameObjectName_RenamesGameObject()
        {
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Player")
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Player"));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_UnderTransform_SetsParent()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WithGameObjectNameAndUnderTransform_AppliesBoth()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Enemy")
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Enemy"));
                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WithGameObjectName_RenamesClonedGameObject()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .WithGameObjectName("Clone")
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.name, Is.EqualTo("Clone"));
                Assert.That(prefabScript.gameObject.name, Is.EqualTo("Prefab"));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WhenContainerParentTransformIsSet_ParentsClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefabScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_UnderTransform_ParentsClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewPrefab(prefab)
                    .UnderTransform(parent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefab.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_AsCached_WithGameObjectNameAndUnderTransform_ConfiguresInstanceOnlyOnce()
        {
            var parent = new GameObject("Parent").transform;
            var first = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .WithGameObjectName("Cached")
                    .UnderTransform(parent)
                    .AsCached();

                first = container.Resolve<Script>();
                var second = container.Resolve<Script>();

                Assert.That(second, Is.SameAs(first));
                Assert.That(first.gameObject.name, Is.EqualTo("Cached"));
                Assert.That(first.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (first != null)
                    UnityEngine.Object.DestroyImmediate(first.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_WhenContainerParentTransformIsSet_SetsParent()
        {
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewPrefab_WhenContainerParentTransformIsSet_ParentsClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var parent = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = parent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewPrefab(prefab)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parent));
                Assert.That(prefab.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Resolve_WhenBindingUnderTransformIsSet_UsesBindingParentInsteadOfContainerParent()
        {
            var containerParent = new GameObject("ContainerParent").transform;
            var bindingParent = new GameObject("BindingParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var container = new Container
                {
                    ParentTransformForGameObjects = containerParent
                };

                container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .UnderTransform(bindingParent)
                    .AsTransient();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(bindingParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(bindingParent.gameObject);
                UnityEngine.Object.DestroyImmediate(containerParent.gameObject);
            }
        }

        [Test]
        public void Resolve_WhenParentContainerHasParentTransform_InheritsParentTransform()
        {
            var parentTransform = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var parentContainer = new Container
                {
                    ParentTransformForGameObjects = parentTransform
                };
                var childContainer = new Container(parentContainer);
                childContainer.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = childContainer.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parentTransform));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parentTransform.gameObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInNewPrefab_WhenParentContainerHasParentTransform_InheritsParentTransform()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var parentTransform = new GameObject("Parent").transform;
            var resolvedScript = default(Script);

            try
            {
                var parentContainer = new Container
                {
                    ParentTransformForGameObjects = parentTransform
                };
                var childContainer = new Container(parentContainer);
                childContainer.Bind<Script>()
                    .FromComponentInNewPrefab(prefabScript)
                    .AsTransient();

                resolvedScript = childContainer.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(parentTransform));
                Assert.That(prefabScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(parentTransform.gameObject);
                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Resolve_WhenSeveralAncestorContainersHaveParentTransforms_UsesNearestParentTransform()
        {
            var rootParent = new GameObject("RootParent").transform;
            var nearestParent = new GameObject("NearestParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var rootContainer = new Container
                {
                    ParentTransformForGameObjects = rootParent
                };
                var parentContainer = new Container(rootContainer)
                {
                    ParentTransformForGameObjects = nearestParent
                };
                var childContainer = new Container(parentContainer);
                childContainer.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = childContainer.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(nearestParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(nearestParent.gameObject);
                UnityEngine.Object.DestroyImmediate(rootParent.gameObject);
            }
        }

        [Test]
        public void Resolve_WhenUnderTransformIsSet_UsesItInsteadOfInheritedParentTransform()
        {
            var inheritedParent = new GameObject("InheritedParent").transform;
            var bindingParent = new GameObject("BindingParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var parentContainer = new Container
                {
                    ParentTransformForGameObjects = inheritedParent
                };
                var childContainer = new Container(parentContainer);
                childContainer.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .UnderTransform(bindingParent)
                    .AsTransient();

                resolvedScript = childContainer.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(bindingParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(bindingParent.gameObject);
                UnityEngine.Object.DestroyImmediate(inheritedParent.gameObject);
            }
        }

        [Test]
        public void Resolve_WhenAncestorBindingIsResolvedFromChild_UsesBindingOwnerParentTransform()
        {
            var bindingOwnerParent = new GameObject("BindingOwnerParent").transform;
            var resolvingContainerParent = new GameObject("ResolvingContainerParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var bindingOwnerContainer = new Container
                {
                    ParentTransformForGameObjects = bindingOwnerParent
                };
                bindingOwnerContainer.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                var resolvingContainer = new Container(bindingOwnerContainer)
                {
                    ParentTransformForGameObjects = resolvingContainerParent
                };

                resolvedScript = resolvingContainer.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(bindingOwnerParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(resolvingContainerParent.gameObject);
                UnityEngine.Object.DestroyImmediate(bindingOwnerParent.gameObject);
            }
        }

        [Test]
        public void Resolve_WhenNearestContextIsGameObjectContext_ParentsInstanceToContextTransform()
        {
            var contextObject = new GameObject("GameObjectContext");
            var resolvedScript = default(Script);

            try
            {
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(context.transform));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Resolve_WhenGameObjectContextHasAncestorParentTransform_UsesContextTransform()
        {
            var contextObject = new GameObject("GameObjectContext");
            var ancestorParent = new GameObject("AncestorParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var parentContainer = new Container
                {
                    ParentTransformForGameObjects = ancestorParent
                };
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize(parentContainer);
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(context.transform));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(ancestorParent.gameObject);
                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Resolve_WhenGameObjectContextDefinesParentTransform_UsesConfiguredParentTransform()
        {
            var contextObject = new GameObject("GameObjectContext");
            var configuredParent = new GameObject("ConfiguredParent").transform;
            var resolvedScript = default(Script);

            try
            {
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(
                    context,
                    parentTransformForGameObjects: configuredParent);
                context.Initialize();
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(configuredParent));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(configuredParent.gameObject);
                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Resolve_WhenNearestContextIsSceneContext_MovesInstanceToContextScene()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var resolvedScript = default(Script);

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);

                var context = contextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.scene, Is.EqualTo(contextScene));
                Assert.That(resolvedScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(contextObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void Resolve_WhenSceneContextHasAncestorParentTransform_MovesInstanceToContextScene()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var ancestorParent = new GameObject("AncestorParent").transform;
            var resolvedScript = default(Script);

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);

                var parentContainer = new Container
                {
                    ParentTransformForGameObjects = ancestorParent
                };
                var context = contextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(context);
                context.Initialize(parentContainer);
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.scene, Is.EqualTo(contextScene));
                Assert.That(resolvedScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(contextObject);
                UnityEngine.Object.DestroyImmediate(ancestorParent.gameObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void Resolve_WhenSceneContextDefinesParentTransform_UsesConfiguredParentTransform()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var configuredParent = new GameObject("ConfiguredParent").transform;
            var resolvedScript = default(Script);

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);
                SceneManager.MoveGameObjectToScene(configuredParent.gameObject, contextScene);

                var context = contextObject.AddComponent<SceneContext>();
                ContextTestUtility.Configure(
                    context,
                    parentTransformForGameObjects: configuredParent);
                context.Initialize();
                context.Install();
                context.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = context.Container.Resolve<Script>();

                Assert.That(resolvedScript.transform.parent, Is.SameAs(configuredParent));
                Assert.That(resolvedScript.gameObject.scene, Is.EqualTo(contextScene));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(configuredParent.gameObject);
                UnityEngine.Object.DestroyImmediate(contextObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void Resolve_WhenAncestorBindingIsResolvedFromGameObjectContext_UsesBindingOwnerSceneContext()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var sceneContextObject = new GameObject("SceneContext");
            var gameObjectContextObject = new GameObject("GameObjectContext");
            var resolvedScript = default(Script);

            try
            {
                SceneManager.MoveGameObjectToScene(sceneContextObject, contextScene);
                SceneManager.MoveGameObjectToScene(gameObjectContextObject, contextScene);

                var sceneContext = sceneContextObject.AddComponent<SceneContext>();
                var gameObjectContext = gameObjectContextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(
                    sceneContext,
                    gameObjectContexts: new[] { gameObjectContext });
                ContextTestUtility.Configure(gameObjectContext);
                sceneContext.Initialize();
                sceneContext.Install();
                sceneContext.Container.Bind<Script>()
                    .FromNewComponentOnNewGameObject()
                    .AsTransient();

                resolvedScript = gameObjectContext.Container.Resolve<Script>();

                Assert.That(resolvedScript.gameObject.scene, Is.EqualTo(contextScene));
                Assert.That(resolvedScript.transform.parent, Is.Null);
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);

                UnityEngine.Object.DestroyImmediate(gameObjectContextObject);
                UnityEngine.Object.DestroyImmediate(sceneContextObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }
    }
}
