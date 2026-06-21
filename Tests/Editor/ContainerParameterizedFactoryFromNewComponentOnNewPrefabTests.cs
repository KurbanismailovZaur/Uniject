using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryFromNewComponentOnNewPrefabTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsGameObject_AddsComponentToClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, Script>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefab));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsComponent_AddsComponentToClonedPrefab()
        {
            var prefabComponent = new GameObject("Prefab").transform;
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Transform, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<Transform, Script>>();
                result = factory.Create(prefabComponent);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabComponent.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefabComponent.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabComponent.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabParameterIsInterface_AddsComponentToClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<ScriptImplementedIInterface>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<IInterface, Script>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<IInterface, Script>>();
                result = factory.Create(prefabScript);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefabScript.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefabAndConcreteResultType_ReturnsConcreteResultAsContract()
        {
            var prefab = new GameObject("Prefab");
            var result = default(IInterface);

            try
            {
                var container = new Container();
                container.BindFactory<GameObject, IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOnNewPrefab()
                    .AsTransient();

                var factory = container.Resolve<Factory<GameObject, IInterface>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(((Component)result).gameObject, Is.Not.SameAs(prefab));
                Assert.That(prefab.GetComponent<ScriptImplementedIInterface>(), Is.Null);
            }
            finally
            {
                if (result is Component resultComponent)
                    UnityEngine.Object.DestroyImmediate(resultComponent.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_InjectsAddedComponent()
        {
            var prefab = new GameObject("Prefab");
            var dependency = new Class();
            var result = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.BindFactory<GameObject, InjectableScript>().FromNewComponentOnNewPrefab().AsTransient();

                var factory = container.Resolve<Factory<GameObject, InjectableScript>>();
                result = factory.Create(prefab);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenParameterTypeIsNotPrefabType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<float, Script>().FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenConcreteResultTypeCanNotBeAddedAsComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, IProduct>()
                    .To<Product>()
                    .FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void BindFactoryWithParameterFromNewComponentOnNewPrefab_WhenResultTypeIsInterfaceWithoutConcreteType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<GameObject, IInterface>().FromNewComponentOnNewPrefab(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenPrefabIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();
            container.BindFactory<GameObject, Script>().FromNewComponentOnNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<GameObject, Script>>();

            Assert.That(
                () => factory.Create(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_WithParameterFactoryFromNewComponentOnNewPrefab_WhenInterfaceParameterIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.BindFactory<IInterface, Script>().FromNewComponentOnNewPrefab().AsTransient();

            var factory = container.Resolve<Factory<IInterface, Script>>();

            Assert.That(
                () => factory.Create(new ClassImplementedIInterface()),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
