using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromNewComponentOnNewGameObjectTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsNotComponent_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().To<IInterface>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_FromNewComponentOnNewGameObject_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<AbstractScript>().To<AbstractScript>().FromNewComponentOnNewGameObject(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_ReturnsComponentOnNewGameObject()
        {
            var resolvedScript = default(Script);

            try
            {
                var container = new Container();
                container.Bind<Script>().To<Script>().FromNewComponentOnNewGameObject();

                resolvedScript = container.Resolve<Script>();

                Assert.That(resolvedScript, Is.Not.Null);
                Assert.That(resolvedScript.gameObject, Is.Not.Null);
                Assert.That(resolvedScript.gameObject.name, Is.EqualTo(nameof(Script)));
                Assert.That(resolvedScript.gameObject.GetComponent<Script>(), Is.SameAs(resolvedScript));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }

        [Test]
        public void Resolve_FromNewComponentOnNewGameObject_InjectsAddedComponent()
        {
            var dependency = new Class();
            var resolvedScript = default(InjectableScript);

            try
            {
                var container = new Container();
                container.Bind<Class>().To<Class>().FromInstance(dependency);
                container.Bind<InjectableScript>().To<InjectableScript>().FromNewComponentOnNewGameObject();

                resolvedScript = container.Resolve<InjectableScript>();

                Assert.That(resolvedScript.Dependency, Is.SameAs(dependency));
            }
            finally
            {
                if (resolvedScript != null)
                    UnityEngine.Object.DestroyImmediate(resolvedScript.gameObject);
            }
        }
    }
}
