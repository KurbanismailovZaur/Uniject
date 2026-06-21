using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromInstanceTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_FromInstance_WhenInstanceIsNull_ThrowsArgumentException()
        {
            var container = new Container();
            Assert.That(
                () => container.Bind<Class>().To<Class>().FromInstance(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_FromInstance_WhenContractIsInterface_ReturnsSameInstance()
        {
            IInterface instance = new ClassImplementedIInterface();

            var container = new Container();
            container.Bind<IInterface>().FromInstance(instance);

            var resolvedInstance = container.Resolve<IInterface>();

            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Resolve_FromInstance_WhenContractIsAbstractClass_ReturnsSameInstance()
        {
            AbstractClass instance = new ClassImplementedAbstractClass();

            var container = new Container();
            container.Bind<AbstractClass>().FromInstance(instance);

            var resolvedInstance = container.Resolve<AbstractClass>();

            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Resolve_FromInstance_ReturnsSameInstance()
        {
            var instance = new Class();

            var container = new Container();
            container.Bind<Class>().To<Class>().FromInstance(instance);

            var resolvedInstance = container.Resolve<Class>();
            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Resolve_FromInstance_WhenBoundUsingNonGenericBind_ReturnsSameInstance()
        {
            IInterface instance = new ClassImplementedIInterface();

            var container = new Container();
            container.Bind(typeof(IInterface)).FromInstance(instance);

            var resolvedInstance = container.Resolve<IInterface>();

            Assert.That(resolvedInstance, Is.SameAs(instance));
        }

        [Test]
        public void Resolve_BindInstance_ReturnsSameInstance()
        {
            var instance = new Class();

            var container = new Container();
            container.BindInstance(instance);

            Assert.That(container.Resolve<Class>(), Is.SameAs(instance));
        }

        [Test]
        public void Resolve_BindInstance_WhenContractIsInterface_ReturnsSameInstance()
        {
            IInterface instance = new ClassImplementedIInterface();

            var container = new Container();
            container.BindInstance(instance);

            Assert.That(container.Resolve<IInterface>(), Is.SameAs(instance));
        }

        [Test]
        public void BindInstance_WhenInstanceIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindInstance<Class>(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_BindInstances_ReturnsInstancesByRuntimeTypes()
        {
            var classInstance = new Class();
            var interfaceImplementation = new ClassImplementedIInterface();

            var container = new Container();
            container.BindInstances(classInstance, interfaceImplementation);

            Assert.That(container.Resolve<Class>(), Is.SameAs(classInstance));
            Assert.That(container.Resolve<ClassImplementedIInterface>(), Is.SameAs(interfaceImplementation));
        }

        [Test]
        public void BindInstances_WhenInstancesArrayIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindInstances(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void BindInstances_WhenInstanceIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindInstances((object)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void BindInstances_WhenRuntimeTypeAlreadyBound_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Class>();

            Assert.That(
                () => container.BindInstances(new Class()),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
