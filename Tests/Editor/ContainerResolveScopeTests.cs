using System;
using NUnit.Framework;
using Uniject;
using Uniject.Attributes;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerResolveScopeTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_AsTransient_ReturnsDifferentInstances()
        {
            var container = new Container();
            container.Bind<Class>().AsTransient();

            var first = container.Resolve<Class>();
            var second = container.Resolve<Class>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_AsCached_ReturnsSameInstance()
        {
            var container = new Container();
            container.Bind<Class>().AsCached();

            var first = container.Resolve<Class>();
            var second = container.Resolve<Class>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }
    }
}
