using System;
using NUnit.Framework;

namespace Uniject.Tests
{
    public class ContainerFactoryFromMethodTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromMethod_InvokesMethodWithContainerForEveryCreate()
        {
            var container = new Container();
            var callsCount = 0;
            var receivedContainer = default(Container);
            container.BindFactory<Product, ProductFactory>().FromMethod(context =>
            {
                callsCount++;
                receivedContainer = context.Container;
                return new Product();
            }).AsCached();
            var factory = container.Resolve<ProductFactory>();

            var first = factory.Create();
            var second = factory.Create();

            Assert.That(receivedContainer, Is.SameAs(container));
            Assert.That(callsCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Create_FromMethod_WhenContractHasConcreteResultType_ReturnsConcreteResult()
        {
            var container = new Container();
            container.BindFactory<IProduct, InterfaceProductFactory>()
                .To<Product>()
                .FromMethod(_ => new Product())
                .AsCached();
            var factory = container.Resolve<InterfaceProductFactory>();

            var instance = factory.Create();

            Assert.That(instance, Is.TypeOf<Product>());
        }

        [Test]
        public void BindFactory_FromMethod_WhenMethodIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<Product, ProductFactory>()
                    .FromMethod((Func<InjectContext, Product>)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_FromMethod_WhenMethodReturnsNull_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindFactory<Product, ProductFactory>().FromMethod(_ => null).AsCached();
            var factory = container.Resolve<ProductFactory>();

            Assert.That(
                () => factory.Create(),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
