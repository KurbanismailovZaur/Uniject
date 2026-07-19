using System;
using NUnit.Framework;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromNewComponentOnTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_FromNewComponentOn_WhenGameObjectIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Script>().FromNewComponentOn(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromNewComponentOn_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<Class>().FromNewComponentOn(gameObject),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Bind_FromNewComponentOn_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<IInterface>().FromNewComponentOn(gameObject),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Bind_FromNewComponentOn_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();

                Assert.That(
                    () => container.Bind<AbstractScript>().FromNewComponentOn(gameObject),
                    Throws.TypeOf<ArgumentException>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_AddsComponentToGivenGameObject()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.Bind<Script>().FromNewComponentOn(gameObject);

                Assert.That(gameObject.GetComponent<Script>(), Is.Null);

                var result = container.Resolve<Script>();

                Assert.That(result.gameObject, Is.SameAs(gameObject));
                Assert.That(gameObject.GetComponent<Script>(), Is.SameAs(result));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_InjectsAddedComponent()
        {
            var gameObject = new GameObject("Target");
            var dependency = new Class();

            try
            {
                var container = new Container();
                container.Bind<Class>().FromInstance(dependency);
                container.Bind<InjectableScript>().FromNewComponentOn(gameObject);

                var result = container.Resolve<InjectableScript>();

                Assert.That(result.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_WhenContractIsInterface_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.Bind<IInterface>()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOn(gameObject);

                var result = container.Resolve<IInterface>();

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That((Component)result, Is.SameAs(gameObject.GetComponent<ScriptImplementedIInterface>()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_WhenBoundUsingNonGenericBind_ReturnsConcreteComponent()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.Bind(typeof(IInterface))
                    .To(typeof(ScriptImplementedIInterface))
                    .FromNewComponentOn(gameObject);

                var result = container.Resolve<IInterface>();

                Assert.That(result, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That((Component)result, Is.SameAs(gameObject.GetComponent<ScriptImplementedIInterface>()));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_AsTransient_AddsComponentForEveryResolve()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.Bind<Script>().FromNewComponentOn(gameObject).AsTransient();

                var first = container.Resolve<Script>();
                var second = container.Resolve<Script>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(gameObject.GetComponents<Script>(), Has.Length.EqualTo(2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_AsCached_AddsComponentOnlyOnce()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.Bind<Script>().FromNewComponentOn(gameObject).AsCached();

                var first = container.Resolve<Script>();
                var second = container.Resolve<Script>();

                Assert.That(second, Is.SameAs(first));
                Assert.That(gameObject.GetComponents<Script>(), Has.Length.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOn_DoesNotChangeGameObjectNameOrParent()
        {
            var originalParent = new GameObject("OriginalParent");
            var containerParent = new GameObject("ContainerParent");
            var gameObject = new GameObject("Target");
            gameObject.transform.SetParent(originalParent.transform);

            try
            {
                var container = new Container(parentTransformForGameObjects: containerParent.transform);
                container.Bind<Script>().FromNewComponentOn(gameObject);

                container.Resolve<Script>();

                Assert.That(gameObject.name, Is.EqualTo("Target"));
                Assert.That(gameObject.transform.parent, Is.SameAs(originalParent.transform));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(originalParent);
                UnityEngine.Object.DestroyImmediate(containerParent);
            }
        }
    }
}
