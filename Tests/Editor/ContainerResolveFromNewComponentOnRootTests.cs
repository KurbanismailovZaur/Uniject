using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class NewComponentOnRootTestScript : MonoBehaviour
    {
    }

    public sealed class NewComponentOnRootTestInterfaceScript : MonoBehaviour, IInterface
    {
    }

    public abstract class NewComponentOnRootTestAbstractScript : MonoBehaviour
    {
    }

    public sealed class NewComponentOnRootTestInjectableScript : MonoBehaviour
    {
        public Class Dependency { get; private set; }

        [Inject]
        public void Construct(Class dependency)
        {
            Dependency = dependency;
        }
    }

    public sealed class NewComponentOnRootTestEntryPoint : MonoBehaviour, IEntryPoint
    {
        public int RunCount { get; private set; }

        public void Run()
        {
            RunCount++;
        }
    }

    public sealed class NewComponentOnRootTestDerivedGameObjectContext : GameObjectContext
    {
    }

    public sealed class NewComponentOnRootTestUnsupportedContext : Context
    {
        protected override void InjectInAllContextGameObjects()
        {
        }
    }

    public class ContainerResolveFromNewComponentOnRootTests
    {
        public enum BindingSurface
        {
            GenericConcrete,
            GenericInterfaceToConcrete,
            NonGenericConcrete,
            NonGenericInterfaceToConcrete
        }

        [Test]
        public void Bind_FromNewComponentOnRoot_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromNewComponentOnRoot(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnRoot_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().FromNewComponentOnRoot(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnRoot_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<NewComponentOnRootTestAbstractScript>().FromNewComponentOnRoot(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnRoot_WhenInterfaceMapsToNonComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>()
                    .To<ClassImplementedIInterface>()
                    .FromNewComponentOnRoot(),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase(BindingSurface.GenericConcrete)]
        [TestCase(BindingSurface.GenericInterfaceToConcrete)]
        [TestCase(BindingSurface.NonGenericConcrete)]
        [TestCase(BindingSurface.NonGenericInterfaceToConcrete)]
        public void Resolve_FromNewComponentOnRoot_OnSupportedBindingSurface_AddsComponentToGameObjectContext(
            BindingSurface bindingSurface)
        {
            var contextObject = new GameObject("GameObjectContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();

                var resolved = ResolveFromBindingSurface(context.Container, bindingSurface);

                Assert.That(resolved, Is.TypeOf<NewComponentOnRootTestInterfaceScript>());
                Assert.That(((Component)resolved).gameObject, Is.SameAs(contextObject));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestInterfaceScript>(),
                    Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WithSceneContext_AddsComponentToContextGameObject()
        {
            var contextObject = new GameObject("SceneContextRoot");

            try
            {
                var context = AddSceneContext(contextObject);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(contextObject));
                Assert.That(contextObject.GetComponent<NewComponentOnRootTestScript>(), Is.SameAs(resolved));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WithDerivedGameObjectContext_AddsComponentToContextGameObject()
        {
            var contextObject = new GameObject("DerivedGameObjectContextRoot");

            try
            {
                var context = contextObject.AddComponent<NewComponentOnRootTestDerivedGameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(contextObject));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenContextIsInactive_AddsInactiveComponent()
        {
            var contextObject = new GameObject("InactiveContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                contextObject.SetActive(false);
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(contextObject));
                Assert.That(resolved.gameObject.activeSelf, Is.False);
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenComponentAlreadyExists_AddsAnotherComponent()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var existing = contextObject.AddComponent<NewComponentOnRootTestScript>();
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved, Is.Not.SameAs(existing));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestScript>(),
                    Has.Length.EqualTo(2));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_InjectsAddedComponent()
        {
            var contextObject = new GameObject("ContextRoot");
            var dependency = new Class();

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<Class>().FromInstance(dependency);
                context.Container.Bind<NewComponentOnRootTestInjectableScript>()
                    .FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestInjectableScript>();

                Assert.That(resolved.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_IgnoresCreateOptionsAndKeepsContextGameObjectUnchanged()
        {
            var physicalParentObject = new GameObject("PhysicalParent");
            var contextObject = new GameObject("NamedContextRoot");
            var configuredParentObject = new GameObject("ConfiguredParent");

            try
            {
                contextObject.transform.SetParent(physicalParentObject.transform);
                var context = AddGameObjectContext(
                    contextObject,
                    configuredParentObject.transform);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(contextObject));
                Assert.That(contextObject.name, Is.EqualTo("NamedContextRoot"));
                Assert.That(contextObject.transform.parent, Is.SameAs(physicalParentObject.transform));
                Assert.That(
                    configuredParentObject.GetComponent<NewComponentOnRootTestScript>(),
                    Is.Null);
            }
            finally
            {
                DestroyGameObjects(physicalParentObject, contextObject, configuredParentObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenBindingOwnerHasNoContext_UsesNearestParentContext()
        {
            var contextObject = new GameObject("ParentContextRoot");
            var dependency = new Class();

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var bindingOwner = new Container(context.Container);
                bindingOwner.Bind<Class>().FromInstance(dependency);
                bindingOwner.Bind<NewComponentOnRootTestInjectableScript>()
                    .FromNewComponentOnRoot();

                var resolved = bindingOwner.Resolve<NewComponentOnRootTestInjectableScript>();

                Assert.That(resolved.gameObject, Is.SameAs(contextObject));
                Assert.That(resolved.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenBindingOwnerHasLocalContext_UsesLocalContextRoot()
        {
            var parentObject = new GameObject("ParentContextRoot");
            var childObject = new GameObject("ChildContextRoot");

            try
            {
                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(parentObject, children: new[] { childContext });
                parentContext.Initialize();
                childContext.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = childContext.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(childObject));
                Assert.That(parentObject.GetComponent<NewComponentOnRootTestScript>(), Is.Null);
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenInheritedBindingIsResolvedFromChild_UsesBindingOwnerRoot()
        {
            var parentObject = new GameObject("ParentContextRoot");
            var childObject = new GameObject("ChildContextRoot");

            try
            {
                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(parentObject, children: new[] { childContext });
                parentContext.Initialize();
                parentContext.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var resolved = childContext.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(resolved.gameObject, Is.SameAs(parentObject));
                Assert.That(childObject.GetComponent<NewComponentOnRootTestScript>(), Is.Null);
            }
            finally
            {
                DestroyGameObjects(parentObject, childObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenContainerHasNoContext_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

            var exception = Assert.Throws<InvalidOperationException>(
                () => container.Resolve<NewComponentOnRootTestScript>());

            Assert.That(
                exception.Message,
                Does.Contain(typeof(NewComponentOnRootTestScript).ToString()));
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenContextIsDestroyed_ThrowsInvalidOperationException()
        {
            var contextObject = new GameObject("DestroyedContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var container = context.Container;
                container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();
                UnityEngine.Object.DestroyImmediate(context);

                Assert.That(
                    () => container.Resolve<NewComponentOnRootTestScript>(),
                    Throws.TypeOf<InvalidOperationException>());
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_WhenContextTypeIsUnsupported_ThrowsInvalidOperationException()
        {
            var contextObject = new GameObject("UnsupportedContextRoot");

            try
            {
                var context = contextObject.AddComponent<NewComponentOnRootTestUnsupportedContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>().FromNewComponentOnRoot();

                var exception = Assert.Throws<InvalidOperationException>(
                    () => context.Container.Resolve<NewComponentOnRootTestScript>());

                Assert.That(
                    exception.Message,
                    Does.Contain(typeof(NewComponentOnRootTestUnsupportedContext).ToString()));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_AsTransient_AddsNewComponentForEveryResolve()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>()
                    .FromNewComponentOnRoot()
                    .AsTransient();

                var first = context.Container.Resolve<NewComponentOnRootTestScript>();
                var second = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestScript>(),
                    Has.Length.EqualTo(2));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_AsCached_ReturnsFirstCreatedComponent()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container.Bind<NewComponentOnRootTestScript>()
                    .FromNewComponentOnRoot()
                    .AsCached();

                var first = context.Container.Resolve<NewComponentOnRootTestScript>();
                var second = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(second, Is.SameAs(first));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestScript>(),
                    Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnRoot_AsCached_WhenFirstResolveFails_RetriesAfterContextAppears()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var bindingOwner = new Container();
                bindingOwner.Bind<NewComponentOnRootTestScript>()
                    .FromNewComponentOnRoot()
                    .AsCached();

                Assert.That(
                    () => bindingOwner.Resolve<NewComponentOnRootTestScript>(),
                    Throws.TypeOf<InvalidOperationException>());

                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                bindingOwner.SetParentContainer(context.Container);

                var first = bindingOwner.Resolve<NewComponentOnRootTestScript>();
                var second = bindingOwner.Resolve<NewComponentOnRootTestScript>();

                Assert.That(first.gameObject, Is.SameAs(contextObject));
                Assert.That(second, Is.SameAs(first));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestScript>(),
                    Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Build_FromNewComponentOnRoot_NonLazyPrewarmsTransientBinding()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Install();
                context.Container.Bind<NewComponentOnRootTestScript>()
                    .FromNewComponentOnRoot()
                    .AsTransient()
                    .NonLazy();

                context.Build();
                var prewarmed = contextObject.GetComponent<NewComponentOnRootTestScript>();
                var first = context.Container.Resolve<NewComponentOnRootTestScript>();
                var second = context.Container.Resolve<NewComponentOnRootTestScript>();

                Assert.That(prewarmed, Is.Not.Null);
                Assert.That(first, Is.SameAs(prewarmed));
                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestScript>(),
                    Has.Length.EqualTo(2));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Build_FromNewComponentOnRoot_AsEntryPoint_AddsAndRunsComponent()
        {
            var contextObject = new GameObject("ContextRoot");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Install();
                context.Container.Bind<NewComponentOnRootTestEntryPoint>()
                    .FromNewComponentOnRoot()
                    .AsEntryPoint();

                context.Build();

                var entryPoint = contextObject.GetComponent<NewComponentOnRootTestEntryPoint>();
                Assert.That(entryPoint, Is.Not.Null);
                Assert.That(entryPoint.RunCount, Is.EqualTo(1));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootTestEntryPoint>(),
                    Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        private static object ResolveFromBindingSurface(
            Container container,
            BindingSurface bindingSurface)
        {
            switch (bindingSurface)
            {
                case BindingSurface.GenericConcrete:
                    container.Bind<NewComponentOnRootTestInterfaceScript>()
                        .FromNewComponentOnRoot();
                    return container.Resolve<NewComponentOnRootTestInterfaceScript>();
                case BindingSurface.GenericInterfaceToConcrete:
                    container.Bind<IInterface>()
                        .To<NewComponentOnRootTestInterfaceScript>()
                        .FromNewComponentOnRoot();
                    return container.Resolve<IInterface>();
                case BindingSurface.NonGenericConcrete:
                    container.Bind(typeof(NewComponentOnRootTestInterfaceScript))
                        .FromNewComponentOnRoot();
                    return container.Resolve(typeof(NewComponentOnRootTestInterfaceScript));
                case BindingSurface.NonGenericInterfaceToConcrete:
                    container.Bind(typeof(IInterface))
                        .To(typeof(NewComponentOnRootTestInterfaceScript))
                        .FromNewComponentOnRoot();
                    return container.Resolve(typeof(IInterface));
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(bindingSurface),
                        bindingSurface,
                        null);
            }
        }

        private static GameObjectContext AddGameObjectContext(
            GameObject gameObject,
            Transform parentTransformForGameObjects = null,
            GameObjectContext[] children = null)
        {
            var context = gameObject.AddComponent<GameObjectContext>();
            ContextTestUtility.Configure(
                context,
                gameObjectContexts: children,
                parentTransformForGameObjects: parentTransformForGameObjects);
            return context;
        }

        private static SceneContext AddSceneContext(GameObject gameObject)
        {
            var context = gameObject.AddComponent<SceneContext>();
            ContextTestUtility.Configure(context);
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
