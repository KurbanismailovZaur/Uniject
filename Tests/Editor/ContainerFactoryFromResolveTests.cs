using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromResolveTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromResolve_ReturnsResolvedResult()
        {
            var container = new Container();
            container.Bind<Product>().AsCached();
            container.BindFactory<Product, ProductFactory>().FromResolve().AsTransient();

            var factory = container.Resolve<ProductFactory>();
            var first = factory.Create();
            var second = factory.Create();
            var resolved = container.Resolve<Product>();

            Assert.That(second, Is.SameAs(first));
            Assert.That(resolved, Is.SameAs(first));
        }
    }
}
