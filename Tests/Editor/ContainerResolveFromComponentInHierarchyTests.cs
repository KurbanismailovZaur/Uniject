using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests
{
    public sealed class ComponentInHierarchyConcreteAbstractScript : AbstractScript
    {
    }

    public sealed class ComponentInHierarchyInjectionProbe : MonoBehaviour
    {
        public int InjectionCount { get; private set; }

        [Inject]
        public void Construct()
        {
            InjectionCount++;
        }
    }

    public sealed class ComponentInHierarchyEntryPoint : MonoBehaviour, IEntryPoint
    {
        public int RunCount { get; private set; }

        public void Run()
        {
            RunCount++;
        }
    }

    public sealed class ComponentInHierarchyUnsupportedContext : Context
    {
        protected override void InjectInAllContextGameObjects()
        {
        }
    }

    public class ContainerResolveFromComponentInHierarchyTests
    {
        public enum BindingSurface
        {
            Concrete,
            Interface,
            InterfaceToConcrete,
            NonGenericInterface,
            NonGenericInterfaceToConcrete
        }

        [Test]
        public void Bind_FromComponentInHierarchy_WhenTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromComponentInHierarchy(),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase(BindingSurface.Concrete)]
        [TestCase(BindingSurface.Interface)]
        [TestCase(BindingSurface.InterfaceToConcrete)]
        [TestCase(BindingSurface.NonGenericInterface)]
        [TestCase(BindingSurface.NonGenericInterfaceToConcrete)]
        public void Resolve_FromComponentInHierarchy_OnSupportedBindingSurface_ReturnsExistingComponent(
            BindingSurface bindingSurface)
        {
            var contextObject = new GameObject("Context");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var existing = contextObject.AddComponent<ScriptImplementedIInterface>();
                object resolved;

                switch (bindingSurface)
                {
                    case BindingSurface.Concrete:
                        context.Container.Bind<ScriptImplementedIInterface>().FromComponentInHierarchy();
                        resolved = context.Container.Resolve<ScriptImplementedIInterface>();
                        break;
                    case BindingSurface.Interface:
                        context.Container.Bind<IInterface>().FromComponentInHierarchy();
                        resolved = context.Container.Resolve<IInterface>();
                        break;
                    case BindingSurface.InterfaceToConcrete:
                        context.Container.Bind<IInterface>()
                            .To<ScriptImplementedIInterface>()
                            .FromComponentInHierarchy();
                        resolved = context.Container.Resolve<IInterface>();
                        break;
                    case BindingSurface.NonGenericInterface:
                        context.Container.Bind(typeof(IInterface)).FromComponentInHierarchy();
                        resolved = context.Container.Resolve(typeof(IInterface));
                        break;
                    case BindingSurface.NonGenericInterfaceToConcrete:
                        context.Container.Bind(typeof(IInterface))
                            .To(typeof(ScriptImplementedIInterface))
                            .FromComponentInHierarchy();
                        resolved = context.Container.Resolve(typeof(IInterface));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bindingSurface), bindingSurface, null);
                }

                Assert.That(resolved, Is.SameAs(existing));
                Assert.That(
                    contextObject.GetComponents<ScriptImplementedIInterface>(),
                    Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenContractIsAbstractComponent_ReturnsDerivedComponent()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var existing = contextObject.AddComponent<ComponentInHierarchyConcreteAbstractScript>();
                context.Container.Bind<AbstractScript>().FromComponentInHierarchy();

                var resolved = context.Container.Resolve<AbstractScript>();

                Assert.That(resolved, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_IncludesInactiveObjectsAndUsesPreorderAndFirstComponent()
        {
            var contextObject = new GameObject("Context");
            var firstBranch = new GameObject("FirstBranch");
            var firstTargetObject = new GameObject("FirstTarget");
            var secondBranch = new GameObject("SecondBranch");

            try
            {
                firstBranch.transform.SetParent(contextObject.transform);
                firstTargetObject.transform.SetParent(firstBranch.transform);
                secondBranch.transform.SetParent(contextObject.transform);
                firstBranch.transform.SetSiblingIndex(0);
                secondBranch.transform.SetSiblingIndex(1);
                firstBranch.SetActive(false);

                var expected = firstTargetObject.AddComponent<Script>();
                firstTargetObject.AddComponent<Script>();
                secondBranch.AddComponent<Script>();

                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = context.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenEveryContextHasComponent_ReturnsLocalComponent()
        {
            var grandparentObject = new GameObject("GrandparentContext");
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                parentObject.transform.SetParent(grandparentObject.transform);
                childObject.transform.SetParent(parentObject.transform);

                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                var grandparentContext = AddGameObjectContext(
                    grandparentObject,
                    children: new[] { parentContext });
                grandparentContext.Initialize();

                grandparentObject.AddComponent<Script>();
                parentObject.AddComponent<Script>();
                var expected = childObject.AddComponent<Script>();
                childContext.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = childContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(grandparentObject, parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenLocalComponentIsMissing_ReturnsParentComponent()
        {
            var grandparentObject = new GameObject("GrandparentContext");
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                parentObject.transform.SetParent(grandparentObject.transform);
                childObject.transform.SetParent(parentObject.transform);

                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                var grandparentContext = AddGameObjectContext(
                    grandparentObject,
                    children: new[] { parentContext });
                grandparentContext.Initialize();

                grandparentObject.AddComponent<Script>();
                var expected = parentObject.AddComponent<Script>();
                childContext.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = childContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(grandparentObject, parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenLocalAndParentComponentsAreMissing_ReturnsGrandparentComponent()
        {
            var grandparentObject = new GameObject("GrandparentContext");
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                parentObject.transform.SetParent(grandparentObject.transform);
                childObject.transform.SetParent(parentObject.transform);

                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                var grandparentContext = AddGameObjectContext(
                    grandparentObject,
                    children: new[] { parentContext });
                grandparentContext.Initialize();

                var expected = grandparentObject.AddComponent<Script>();
                childContext.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = childContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(grandparentObject, parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenParentOwnsBinding_ExcludesLogicalChildAndGrandchild()
        {
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");
            var grandchildObject = new GameObject("GrandchildContext");
            var fallbackObject = new GameObject("ParentFallback");

            try
            {
                childObject.transform.SetParent(parentObject.transform);
                grandchildObject.transform.SetParent(parentObject.transform);
                fallbackObject.transform.SetParent(parentObject.transform);
                childObject.transform.SetSiblingIndex(0);
                grandchildObject.transform.SetSiblingIndex(1);
                fallbackObject.transform.SetSiblingIndex(2);

                var grandchildContext = AddGameObjectContext(grandchildObject);
                var childContext = AddGameObjectContext(
                    childObject,
                    children: new[] { grandchildContext });
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                parentContext.Initialize();

                childObject.AddComponent<Script>();
                grandchildObject.AddComponent<ScriptImplementedIInterface>();
                var expectedScript = fallbackObject.AddComponent<Script>();
                var expectedInterface = fallbackObject.AddComponent<ScriptImplementedIInterface>();
                parentContext.Container.Bind<Script>().FromComponentInHierarchy();
                parentContext.Container.Bind<IInterface>().FromComponentInHierarchy();

                var resolvedScript = grandchildContext.Container.Resolve<Script>();
                var resolvedInterface = grandchildContext.Container.Resolve<IInterface>();

                Assert.That(resolvedScript, Is.SameAs(expectedScript));
                Assert.That(resolvedInterface, Is.SameAs(expectedInterface));
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject, grandchildObject, fallbackObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenNestedContextIsLogicallyUnrelated_SearchesItsSubtree()
        {
            var parentObject = new GameObject("ParentContext");
            var unrelatedObject = new GameObject("UnrelatedContext");

            try
            {
                unrelatedObject.transform.SetParent(parentObject.transform);

                var parentContext = AddGameObjectContext(parentObject);
                var unrelatedContext = AddGameObjectContext(unrelatedObject);
                parentContext.Initialize();
                unrelatedContext.Initialize();

                var expected = unrelatedObject.AddComponent<Script>();
                parentContext.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = parentContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject, unrelatedObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenChildHasExternalParentTransform_ParentCanSearchIt()
        {
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");
            var externalObject = new GameObject("ChildExternalParent");

            try
            {
                childObject.transform.SetParent(parentObject.transform);
                externalObject.transform.SetParent(parentObject.transform);
                childObject.transform.SetSiblingIndex(0);
                externalObject.transform.SetSiblingIndex(1);

                var childContext = AddGameObjectContext(
                    childObject,
                    parentTransformForGameObjects: externalObject.transform);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                parentContext.Initialize();

                var expected = externalObject.AddComponent<Script>();
                parentContext.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = parentContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject, externalObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenGameObjectContextHasExternalParentTransform_SearchesIt()
        {
            var contextObject = new GameObject("Context");
            var externalObject = new GameObject("ExternalParent");

            try
            {
                var context = AddGameObjectContext(
                    contextObject,
                    parentTransformForGameObjects: externalObject.transform);
                context.Initialize();
                var expected = externalObject.AddComponent<Script>();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = context.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(contextObject, externalObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenConfiguredRootIsInsideLogicalChild_DoesNotBypassBoundary()
        {
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");
            var configuredRoot = new GameObject("ConfiguredRootInsideChild");

            try
            {
                childObject.transform.SetParent(parentObject.transform);
                configuredRoot.transform.SetParent(childObject.transform);

                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext },
                    parentTransformForGameObjects: configuredRoot.transform);
                parentContext.Initialize();

                configuredRoot.AddComponent<Script>();
                parentContext.Container.Bind<Script>().FromComponentInHierarchy();

                Assert.That(
                    () => parentContext.Container.Resolve<Script>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject, configuredRoot);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_FromSceneContext_SearchesSeparateSceneRoot()
        {
            var previewScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var serviceObject = new GameObject("Service");

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, previewScene);
                SceneManager.MoveGameObjectToScene(serviceObject, previewScene);

                var context = AddSceneContext(contextObject);
                context.Initialize();
                var expected = serviceObject.AddComponent<Script>();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = context.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(contextObject, serviceObject);

                if (previewScene.IsValid() && previewScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenSceneContextHasExternalParentTransform_SearchesIt()
        {
            var previewScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var externalObject = new GameObject("ExternalParentTransform");

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, previewScene);

                var context = AddSceneContext(
                    contextObject,
                    parentTransformForGameObjects: externalObject.transform);
                context.Initialize();
                var expected = externalObject.AddComponent<Script>();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var resolved = context.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(contextObject, externalObject);

                if (previewScene.IsValid() && previewScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_FromSceneContext_DoesNotSearchLogicalChildRoot()
        {
            var previewScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, previewScene);
                SceneManager.MoveGameObjectToScene(childObject, previewScene);

                var childContext = AddGameObjectContext(childObject);
                var context = AddSceneContext(contextObject, children: new[] { childContext });
                context.Initialize();
                childObject.AddComponent<Script>();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                Assert.That(
                    () => context.Container.Resolve<Script>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(contextObject, childObject);

                if (previewScene.IsValid() && previewScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenBindingOwnerHasNoContext_StartsAtNearestParentContext()
        {
            var contextObject = new GameObject("ParentContext");

            try
            {
                var expected = contextObject.AddComponent<Script>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var bindingOwner = new Container(context.Container);
                bindingOwner.Bind<Script>().FromComponentInHierarchy();

                var resolved = bindingOwner.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenContainerHasNoContext_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Script>().FromComponentInHierarchy();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenComponentIsMissing_ReportsTypeAndCheckedContext()
        {
            var contextObject = new GameObject("CheckedContext");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var exception = Assert.Throws<InvalidOperationException>(
                    () => context.Container.Resolve<Script>());

                Assert.That(exception.Message, Does.Contain(typeof(Script).ToString()));
                Assert.That(exception.Message, Does.Contain(contextObject.name));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenOwnerContextIsDestroyed_ThrowsInvalidOperationException()
        {
            var contextObject = new GameObject("DestroyedContext");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var container = context.Container;
                container.Bind<Script>().FromComponentInHierarchy();

                UnityEngine.Object.DestroyImmediate(context);

                Assert.That(
                    () => container.Resolve<Script>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenIntermediateParentContextIsDestroyed_ThrowsBeforeGrandparent()
        {
            var grandparentObject = new GameObject("GrandparentContext");
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                var grandparentContext = AddGameObjectContext(
                    grandparentObject,
                    children: new[] { parentContext });
                grandparentContext.Initialize();

                grandparentObject.AddComponent<Script>();
                childContext.Container.Bind<Script>().FromComponentInHierarchy();
                UnityEngine.Object.DestroyImmediate(parentContext);

                Assert.That(
                    () => childContext.Container.Resolve<Script>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(grandparentObject, parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenContextTypeIsUnsupported_ThrowsInvalidOperationException()
        {
            var contextObject = new GameObject("UnsupportedContext");

            try
            {
                var context = contextObject.AddComponent<ComponentInHierarchyUnsupportedContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy();

                var exception = Assert.Throws<InvalidOperationException>(
                    () => context.Container.Resolve<Script>());

                Assert.That(
                    exception.Message,
                    Does.Contain(typeof(ComponentInHierarchyUnsupportedContext).ToString()));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenConfiguredParentIsDestroyed_SkipsItAndSearchesParentContext()
        {
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");
            var configuredParentObject = new GameObject("ConfiguredParent");

            try
            {
                childObject.transform.SetParent(parentObject.transform);

                var childContext = AddGameObjectContext(
                    childObject,
                    parentTransformForGameObjects: configuredParentObject.transform);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                parentContext.Initialize();

                var expected = parentObject.AddComponent<Script>();
                childContext.Container.Bind<Script>().FromComponentInHierarchy();
                UnityEngine.Object.DestroyImmediate(configuredParentObject);

                var resolved = childContext.Container.Resolve<Script>();

                Assert.That(resolved, Is.SameAs(expected));
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject, configuredParentObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_WhenContainerHierarchyHasCycle_ThrowsInvalidOperationException()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy();
                context.Container.SetParentContainer(context.Container);

                Assert.That(
                    () => context.Container.Resolve<Script>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_AsTransient_RepeatsSearch()
        {
            var contextObject = new GameObject("Context");
            var firstObject = new GameObject("First");
            var secondObject = new GameObject("Second");

            try
            {
                firstObject.transform.SetParent(contextObject.transform);
                secondObject.transform.SetParent(contextObject.transform);
                firstObject.transform.SetSiblingIndex(0);
                secondObject.transform.SetSiblingIndex(1);

                var first = firstObject.AddComponent<Script>();
                var second = secondObject.AddComponent<Script>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy().AsTransient();

                var firstResolved = context.Container.Resolve<Script>();
                firstObject.transform.SetParent(null);
                var secondResolved = context.Container.Resolve<Script>();

                Assert.That(firstResolved, Is.SameAs(first));
                Assert.That(secondResolved, Is.SameAs(second));
            }
            finally
            {
                DestroyGameObjects(contextObject, firstObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_AsCached_ReturnsFirstSuccessfulResult()
        {
            var contextObject = new GameObject("Context");
            var firstObject = new GameObject("First");
            var secondObject = new GameObject("Second");

            try
            {
                firstObject.transform.SetParent(contextObject.transform);
                secondObject.transform.SetParent(contextObject.transform);
                firstObject.transform.SetSiblingIndex(0);
                secondObject.transform.SetSiblingIndex(1);

                var first = firstObject.AddComponent<Script>();
                secondObject.AddComponent<Script>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Script>().FromComponentInHierarchy().AsCached();

                var firstResolved = context.Container.Resolve<Script>();
                firstObject.transform.SetParent(null);
                var secondResolved = context.Container.Resolve<Script>();

                Assert.That(firstResolved, Is.SameAs(first));
                Assert.That(secondResolved, Is.SameAs(first));
            }
            finally
            {
                DestroyGameObjects(contextObject, firstObject);
            }
        }

        [Test]
        public void Build_FromComponentInHierarchy_NonLazyPrewarmsTransientBinding()
        {
            var contextObject = new GameObject("Context");
            var firstObject = new GameObject("First");
            var secondObject = new GameObject("Second");

            try
            {
                firstObject.transform.SetParent(contextObject.transform);
                secondObject.transform.SetParent(contextObject.transform);
                firstObject.transform.SetSiblingIndex(0);
                secondObject.transform.SetSiblingIndex(1);

                var first = firstObject.AddComponent<Script>();
                var second = secondObject.AddComponent<Script>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Install();
                context.Container.Bind<Script>()
                    .FromComponentInHierarchy()
                    .AsTransient()
                    .NonLazy();

                context.Build();
                firstObject.transform.SetParent(null);
                var firstResolved = context.Container.Resolve<Script>();
                var secondResolved = context.Container.Resolve<Script>();

                Assert.That(firstResolved, Is.SameAs(first));
                Assert.That(secondResolved, Is.SameAs(second));
            }
            finally
            {
                DestroyGameObjects(contextObject, firstObject);
            }
        }

        [Test]
        public void Build_FromComponentInHierarchy_AsEntryPoint_RunsFoundComponent()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var entryPoint = contextObject.AddComponent<ComponentInHierarchyEntryPoint>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Install();
                context.Container.Bind<ComponentInHierarchyEntryPoint>()
                    .FromComponentInHierarchy()
                    .AsEntryPoint();

                context.Build();

                Assert.That(entryPoint.RunCount, Is.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromComponentInHierarchy_DoesNotInjectFoundComponent()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var probe = contextObject.AddComponent<ComponentInHierarchyInjectionProbe>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<ComponentInHierarchyInjectionProbe>()
                    .FromComponentInHierarchy();

                var resolved = context.Container.Resolve<ComponentInHierarchyInjectionProbe>();

                Assert.That(resolved, Is.SameAs(probe));
                Assert.That(probe.InjectionCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        private static GameObjectContext AddGameObjectContext(
            GameObject gameObject,
            GameObjectContext[] children = null,
            Transform parentTransformForGameObjects = null)
        {
            var context = gameObject.AddComponent<GameObjectContext>();
            ContextTestUtility.Configure(
                context,
                gameObjectContexts: children,
                parentTransformForGameObjects: parentTransformForGameObjects);
            return context;
        }

        private static SceneContext AddSceneContext(
            GameObject gameObject,
            GameObjectContext[] children = null,
            Transform parentTransformForGameObjects = null)
        {
            var context = gameObject.AddComponent<SceneContext>();
            ContextTestUtility.Configure(
                context,
                gameObjectContexts: children,
                parentTransformForGameObjects: parentTransformForGameObjects);
            return context;
        }

        private static void DestroyGameObjects(params GameObject[] gameObjects)
        {
            for (var i = gameObjects.Length - 1; i >= 0; i--)
            {
                if (gameObjects[i] != null)
                    UnityEngine.Object.DestroyImmediate(gameObjects[i]);
            }
        }
    }
}
