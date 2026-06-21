using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerBindingTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_WhenTypeAlreadyBound_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Class>();

            Assert.That(
                () => container.Bind<Class>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Bind_WhenDifferentTypesAreBound_DoesNotThrow()
        {
            var container = new Container();

            Assert.That(() =>
            {
                container.Bind<Class>().To<Class>();
                container.Bind<IInterface>().To<ClassImplementedIInterface>();
            }, Throws.Nothing);
        }

        [Test]
        public void Resolve_WhenSameConcreteTypeIsBoundToDifferentContracts_ReturnsInstances()
        {
            var container = new Container();

            container.Bind<ClassImplementedIInterface>().To<ClassImplementedIInterface>();
            container.Bind<IInterface>().To<ClassImplementedIInterface>();

            var concreteInstance = container.Resolve<ClassImplementedIInterface>();
            var interfaceInstance = container.Resolve<IInterface>();

            Assert.That(concreteInstance, Is.Not.Null);
            Assert.That(interfaceInstance, Is.Not.Null);
        }

        [Test]
        public void Resolve_WhenTypeWasBound_ReturnsInstance()
        {
            var container = new Container();
            container.Bind<Class>();

            var instance = container.Resolve<Class>();
            Assert.That(instance, Is.Not.Null);
        }

        [Test]
        public void Bind_WhenContractTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_WhenTypeAlreadyBoundUsingNonGenericBind_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind(typeof(Class));

            Assert.That(
                () => container.Bind<Class>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Bind_WhenTypeAlreadyBoundUsingGenericBind_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Class>();

            Assert.That(
                () => container.Bind(typeof(Class)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_WhenTypeWasBoundUsingNonGenericBind_ReturnsInstance()
        {
            var container = new Container();
            container.Bind(typeof(Class));

            var instance = container.Resolve<Class>();

            Assert.That(instance, Is.TypeOf<Class>());
        }

        [Test]
        public void Resolve_WhenNonGenericBindUsesNonGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind(typeof(IInterface)).To(typeof(ClassImplementedIInterface));

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Resolve_WhenNonGenericBindUsesGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind(typeof(IInterface)).To<ClassImplementedIInterface>();

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Resolve_WhenGenericBindUsesNonGenericTo_ReturnsConcreteInstance()
        {
            var container = new Container();
            container.Bind<IInterface>().To(typeof(ClassImplementedIInterface));

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.TypeOf<ClassImplementedIInterface>());
        }

        [Test]
        public void Bind_To_WhenConcreteTypeIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(typeof(IInterface)).To(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_To_WhenConcreteTypeIsNotAssignableToContract_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind(typeof(IInterface)).To(typeof(Class)),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Bind_To_WhenGenericBindUsesNotAssignableConcreteType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IInterface>().To(typeof(Class)),
                Throws.TypeOf<ArgumentException>());
        }
    }
}
