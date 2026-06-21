using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveFromResolveTests : ContainerResolveTestFixture
    {
        [Test]
        public void Bind_FromResolve_WhenContractTypeEqualsConcreteType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().To<Class>().FromResolve(),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteTypeIsNotBound_ThrowsException()
        {
            var container = new Container();
            container.Bind<IInterface>().To<ClassImplementedIInterface>().FromResolve();

            Assert.That(
                () => container.Resolve<IInterface>(),
                Throws.Exception);
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteBindingIsTransient_ReturnsDifferentInstances()
        {
            var container = new Container();
            container.Bind<ClassImplementedIInterface>();
            container.Bind<IInterface>().To<ClassImplementedIInterface>().FromResolve();

            var first = container.Resolve<IInterface>();
            var second = container.Resolve<IInterface>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
        }
    }
}
