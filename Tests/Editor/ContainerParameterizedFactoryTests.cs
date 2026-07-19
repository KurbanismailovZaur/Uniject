using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryTests : ContainerFactoryTestFixture
    {
        [Test]
        public void BindFactoryWithParameter_WhenFactoryWasBound_ResolvesFactoryByFactoryType()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script, GameObjectScriptFactory>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<GameObjectScriptFactory>();
                result = factory.Create(prefabScript.gameObject);

                Assert.That(factory, Is.Not.Null);
                Assert.That(factory is IFactory<GameObject, Script>, Is.True);
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void ResolveFactoryWithParameter_AsTransient_ReturnsDifferentFactories()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script, GameObjectScriptFactory>().FromComponentInNewPrefab().AsTransient();

            var first = container.Resolve<GameObjectScriptFactory>();
            var second = container.Resolve<GameObjectScriptFactory>();

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void ResolveFactoryWithParameter_AsCached_ReturnsSameFactoryButCreatesTransientResults()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var firstResult = default(Script);
            var secondResult = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script, GameObjectScriptFactory>().FromComponentInNewPrefab().AsCached();

                var firstFactory = container.Resolve<GameObjectScriptFactory>();
                var secondFactory = container.Resolve<GameObjectScriptFactory>();
                firstResult = firstFactory.Create(prefabScript.gameObject);
                secondResult = secondFactory.Create(prefabScript.gameObject);

                Assert.That(secondFactory, Is.SameAs(firstFactory));
                Assert.That(firstResult, Is.Not.SameAs(secondResult));
                Assert.That(firstResult.gameObject, Is.Not.SameAs(secondResult.gameObject));
            }
            finally
            {
                if (firstResult != null)
                    UnityEngine.Object.DestroyImmediate(firstResult.gameObject);

                if (secondResult != null)
                    UnityEngine.Object.DestroyImmediate(secondResult.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void ResolveFactoryWithParameter_WhenSourceIsNotConfigured_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindFactory<Class, Product, ClassProductFactory>();

            var expectedMessage =
                $"Source for parameterized factory {typeof(ClassProductFactory)} is not configured. " +
                "Use FromMethod(), FromComponentInNewPrefab(), FromNewComponentOnNewPrefab(), " +
                "or FromFactory<TCustomFactory>().";

            Assert.That(
                () => container.Resolve<ClassProductFactory>(),
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo(expectedMessage));
        }
    }
}
