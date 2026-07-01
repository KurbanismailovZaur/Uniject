using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryFromComponentInNewPrefabTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_WithParameterFactoryAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface, GameObjectInterfaceFactory>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<GameObjectInterfaceFactory>();
                result = factory.Create(prefabScript.gameObject);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabParameterIsComponent_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, Script, ScriptScriptFactory>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<ScriptScriptFactory>();
                result = factory.Create(prefabScript);

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
        public void Create_WithParameterFactory_WhenPrefabParameterIsInterface_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, IInterface, InterfaceInterfaceFactory>()
                    .To<ScriptImplementedIInterface>()
                    .FromComponentInNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<InterfaceInterfaceFactory>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void BindFactoryWithParameter_WhenParameterTypeIsNotPrefabType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<float, Script, FloatScriptFactory>().FromComponentInNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameter_WhenResultTypeIsNotComponentOrInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, Product, GameObjectProductFactory>().FromComponentInNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script, GameObjectScriptFactory>().FromComponentInNewPrefab().AsTransient();

            var factory = container.Resolve<GameObjectScriptFactory>();

            Assert.That(
                () => factory.Create(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_WithParameterFactory_WhenPrefabDoesNotHaveRequestedComponent_ThrowsArgumentException()
        {
            var prefab = new GameObject("Prefab");

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script, GameObjectScriptFactory>().FromComponentInNewPrefab().AsTransient();

                var factory = container.Resolve<GameObjectScriptFactory>();

                Assert.That(
                    () => factory.Create(prefab),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactory_WhenInterfaceParameterIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.BindFactory<IInterface, Script, InterfaceScriptFactory>().FromComponentInNewPrefab().AsTransient();

            var factory = container.Resolve<InterfaceScriptFactory>();

            Assert.That(
                () => factory.Create(new ClassImplementedIInterface()),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
