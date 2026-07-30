using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using UnityEngine;

namespace Uniject.Tests
{
    public sealed class NewComponentOnRootFactoryDependency
    {
    }

    public sealed class NewComponentOnRootFactoryInjectableTarget : MonoBehaviour
    {
        public NewComponentOnRootFactoryDependency Dependency { get; private set; }

        [Inject]
        public void Construct(NewComponentOnRootFactoryDependency dependency)
        {
            Dependency = dependency;
        }
    }

    public sealed class NewComponentOnRootFactoryTarget : MonoBehaviour
    {
    }

    public interface INewComponentOnRootFactoryTarget
    {
    }

    public sealed class NewComponentOnRootFactoryInterfaceTarget : MonoBehaviour, INewComponentOnRootFactoryTarget
    {
    }

    public class ContainerFactoryFromNewComponentOnRootTests : ContainerFactoryTestFixture
    {
        private sealed class NewComponentOnRootInjectableTargetFactory
            : Factory<NewComponentOnRootFactoryInjectableTarget>
        {
            public NewComponentOnRootInjectableTargetFactory()
            {
            }
        }

        private sealed class NewComponentOnRootTargetFactory : Factory<NewComponentOnRootFactoryTarget>
        {
            public NewComponentOnRootTargetFactory()
            {
            }
        }

        private sealed class NewComponentOnRootInterfaceTargetFactory
            : Factory<INewComponentOnRootFactoryTarget>
        {
            public NewComponentOnRootInterfaceTargetFactory()
            {
            }
        }

        [Test]
        public void Create_FromNewComponentOnRoot_AddsConcreteComponentToOwnerContextAndInjectsIt()
        {
            var contextObject = new GameObject("OwnerContext");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                var dependency = new NewComponentOnRootFactoryDependency();
                context.Container.Bind<NewComponentOnRootFactoryDependency>()
                    .FromInstance(dependency);
                context.Container
                    .BindFactory<NewComponentOnRootFactoryInjectableTarget, NewComponentOnRootInjectableTargetFactory>()
                    .FromNewComponentOnRoot()
                    .AsTransient();

                var result = context.Container
                    .Resolve<NewComponentOnRootInjectableTargetFactory>()
                    .Create();

                Assert.That(result.gameObject, Is.SameAs(contextObject));
                Assert.That(result.Dependency, Is.SameAs(dependency));
                Assert.That(
                    contextObject.GetComponent<NewComponentOnRootFactoryInjectableTarget>(),
                    Is.SameAs(result));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnRoot_WhenInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var contextObject = new GameObject("OwnerContext");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container
                    .BindFactory<INewComponentOnRootFactoryTarget, NewComponentOnRootInterfaceTargetFactory>()
                    .To<NewComponentOnRootFactoryInterfaceTarget>()
                    .FromNewComponentOnRoot()
                    .AsTransient();

                var result = context.Container
                    .Resolve<NewComponentOnRootInterfaceTargetFactory>()
                    .Create();

                Assert.That(result, Is.TypeOf<NewComponentOnRootFactoryInterfaceTarget>());
                Assert.That(((Component)result).gameObject, Is.SameAs(contextObject));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnRoot_WhenFactoryIsCached_ReturnsSameFactoryAndAddsNewComponentForEveryCreate()
        {
            var contextObject = new GameObject("OwnerContext");

            try
            {
                var context = AddGameObjectContext(contextObject);
                context.Initialize();
                context.Container
                    .BindFactory<NewComponentOnRootFactoryTarget, NewComponentOnRootTargetFactory>()
                    .FromNewComponentOnRoot()
                    .AsCached();

                var firstFactory = context.Container.Resolve<NewComponentOnRootTargetFactory>();
                var secondFactory = context.Container.Resolve<NewComponentOnRootTargetFactory>();
                var first = firstFactory.Create();
                var second = firstFactory.Create();

                Assert.That(secondFactory, Is.SameAs(firstFactory));
                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(first.gameObject, Is.SameAs(contextObject));
                Assert.That(second.gameObject, Is.SameAs(contextObject));
                Assert.That(
                    contextObject.GetComponents<NewComponentOnRootFactoryTarget>(),
                    Has.Length.EqualTo(2));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnRoot_WhenFactoryBindingIsInherited_UsesBindingOwnerContext()
        {
            var parentObject = new GameObject("ParentContext");
            var childObject = new GameObject("ChildContext");

            try
            {
                childObject.transform.SetParent(parentObject.transform);
                var childContext = AddGameObjectContext(childObject);
                var parentContext = AddGameObjectContext(
                    parentObject,
                    children: new[] { childContext });
                parentContext.Initialize();
                parentContext.Container
                    .BindFactory<NewComponentOnRootFactoryTarget, NewComponentOnRootTargetFactory>()
                    .FromNewComponentOnRoot()
                    .AsTransient();

                var result = childContext.Container
                    .Resolve<NewComponentOnRootTargetFactory>()
                    .Create();

                Assert.That(result.gameObject, Is.SameAs(parentObject));
                Assert.That(childObject.GetComponent<NewComponentOnRootFactoryTarget>(), Is.Null);
            }
            finally
            {
                DestroyGameObjects(parentObject);
            }
        }

        [Test]
        public void Create_FromNewComponentOnRoot_WhenContainerHasNoContext_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container
                .BindFactory<NewComponentOnRootFactoryTarget, NewComponentOnRootTargetFactory>()
                .FromNewComponentOnRoot()
                .AsTransient();
            var factory = container.Resolve<NewComponentOnRootTargetFactory>();

            var exception = Assert.Throws<InvalidOperationException>(() => factory.Create());

            Assert.That(
                exception.Message,
                Does.Contain(typeof(NewComponentOnRootFactoryTarget).ToString()));
        }

        private static GameObjectContext AddGameObjectContext(
            GameObject gameObject,
            GameObjectContext[] children = null)
        {
            var context = gameObject.AddComponent<GameObjectContext>();
            ContextTestUtility.Configure(context, gameObjectContexts: children);
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
