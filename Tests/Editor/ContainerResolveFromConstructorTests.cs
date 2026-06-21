using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromConstructorTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsInterface_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<IInterface>().To<IInterface>().FromConstructor();

            Assert.That(
                () => container.Resolve<IInterface>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsAbstract_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<AbstractClass>().To<AbstractClass>().FromConstructor();

            Assert.That(
                () => container.Resolve<AbstractClass>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenConcreteTypeIsComponent_ThrowsArgumentException()
        {
            var container = new Container();
            container.Bind<Script>().To<Script>().FromConstructor();

            Assert.That(
                () => container.Resolve<Script>(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromConstructor_WhenNoPublicConstructor_ThrowsException()
        {
            var container = new Container();
            container.Bind<ClassWithPrivateConstructor>().To<ClassWithPrivateConstructor>().FromConstructor();

            Assert.That(
                () => container.Resolve<ClassWithPrivateConstructor>(),
                Throws.Exception);
        }

        [Test]
        public void Resolve_FromConstructor_ReturnsNewInstance()
        {
            var container = new Container();
            container.Bind<Class>().To<Class>().FromConstructor();

            var instance = container.Resolve<Class>();
            Assert.IsNotNull(instance);
        }
    }
}
