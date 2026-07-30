using NUnit.Framework;
using Uniject.Contexts;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromComponentInHierarchyTests : ContainerFactoryTestFixture
    {
        private sealed class ComponentInHierarchyInterfaceFactory : Factory<IInterface>
        {
            public ComponentInHierarchyInterfaceFactory()
            {
            }
        }

        [Test]
        public void Create_FromComponentInHierarchy_WhenFactoryIsCached_RepeatsSearchForEveryCreate()
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
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.BindFactory<Script, ScriptFactory>()
                    .FromComponentInHierarchy()
                    .AsCached();

                var firstFactory = context.Container.Resolve<ScriptFactory>();
                var secondFactory = context.Container.Resolve<ScriptFactory>();
                var firstResult = firstFactory.Create();
                firstObject.transform.SetParent(null);
                var secondResult = firstFactory.Create();

                Assert.That(secondFactory, Is.SameAs(firstFactory));
                Assert.That(firstResult, Is.SameAs(first));
                Assert.That(secondResult, Is.SameAs(second));
            }
            finally
            {
                DestroyGameObjects(contextObject, firstObject);
            }
        }

        [Test]
        public void Create_FromComponentInHierarchy_WhenInterfaceMapsToConcrete_ReturnsConcreteComponent()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var existing = contextObject.AddComponent<ScriptImplementedIInterface>();
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.BindFactory<IInterface, ComponentInHierarchyInterfaceFactory>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInHierarchy()
                    .AsTransient();

                var result = context.Container
                    .Resolve<ComponentInHierarchyInterfaceFactory>()
                    .Create();

                Assert.That(result, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
        }

        [Test]
        public void Create_FromComponentInHierarchy_WhenResultTypeIsInterface_ReturnsImplementingComponent()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var existing = contextObject.AddComponent<ScriptImplementedIInterface>();
                var context = contextObject.AddComponent<GameObjectContext>();
                ContextTestUtility.Configure(context);
                context.Initialize();
                context.Container.BindFactory<IInterface, ComponentInHierarchyInterfaceFactory>()
                    .FromComponentInHierarchy()
                    .AsTransient();

                var result = context.Container
                    .Resolve<ComponentInHierarchyInterfaceFactory>()
                    .Create();

                Assert.That(result, Is.SameAs(existing));
            }
            finally
            {
                DestroyGameObjects(contextObject);
            }
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
