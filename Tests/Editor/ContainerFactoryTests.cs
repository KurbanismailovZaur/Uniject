using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryTests : ContainerFactoryTestFixture
    {
        [Test]
        public void BindFactory_WhenFactoryWasBound_ResolvesFactoryByFactoryType()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().FromConstructor().AsTransient();

            var factory = container.Resolve<ProductFactory>();
            var product = factory.Create();

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory is IFactory<Product>, Is.True);
            Assert.That(product, Is.TypeOf<Product>());
        }

        [Test]
        public void ResolveFactory_AsTransient_ReturnsDifferentFactories()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().FromConstructor().AsTransient();

            var first = container.Resolve<ProductFactory>();
            var second = container.Resolve<ProductFactory>();

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void ResolveFactory_AsCached_ReturnsSameFactoryButCreatesTransientResults()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().FromConstructor().AsCached();

            var firstFactory = container.Resolve<ProductFactory>();
            var secondFactory = container.Resolve<ProductFactory>();
            var firstProduct = firstFactory.Create();
            var secondProduct = secondFactory.Create();

            Assert.That(secondFactory, Is.SameAs(firstFactory));
            Assert.That(firstProduct, Is.Not.SameAs(secondProduct));
        }

        [Test]
        public void Create_WhenFactoryUsesConcreteResultType_ReturnsConcreteResult()
        {
            var container = new Container();
            container.BindFactory<IProduct, InterfaceProductFactory>().To<Product>().FromConstructor().AsTransient();

            var factory = container.Resolve<InterfaceProductFactory>();
            var product = factory.Create();

            Assert.That(product, Is.TypeOf<Product>());
        }

        [Test]
        public void BindFactory_WhenFactoryTypeAlreadyBoundByFactoryBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>();

            Assert.That(
                () => container.BindFactory<Product, ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void BindFactory_WhenFactoryTypeAlreadyBoundByRegularBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<ProductFactory>();

            Assert.That(
                () => container.BindFactory<Product, ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Bind_WhenFactoryTypeAlreadyBoundByFactoryBinding_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>();

            Assert.That(
                () => container.Bind<ProductFactory>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_WhenContainerIsCreated_ReturnsContainerAndObjectBuilder()
        {
            var container = new Container();

            Assert.That(container.Resolve<Container>(), Is.SameAs(container));
            Assert.That(container.Resolve<IObjectBuilder>(), Is.SameAs(container));
        }
    }
}
