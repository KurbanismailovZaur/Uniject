using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromCustomFactoryTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromFactory_UsesCustomFactory()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().To<Product>().FromFactory<CustomProductFactory>().AsTransient();

            var product = container.Resolve<ProductFactory>().Create();

            Assert.That(product, Is.TypeOf<Product>());
        }
    }
}
