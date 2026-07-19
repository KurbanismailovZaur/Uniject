using System;
using NUnit.Framework;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public class ContainerParameterizedFactoryFromMethodTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromMethod_InvokesMethodWithContainerAndParameterForEveryCreate()
        {
            var container = new Container();
            var callsCount = 0;
            var receivedContainer = default(Container);
            var receivedParameter = default(Class);
            container.BindFactory<Class, Product, ClassProductFactory>().FromMethod((currentContainer, parameter) =>
            {
                callsCount++;
                receivedContainer = currentContainer;
                receivedParameter = parameter;
                return new Product();
            }).AsCached();
            var factory = container.Resolve<ClassProductFactory>();
            var firstParameter = new Class();
            var secondParameter = new Class();

            var first = factory.Create(firstParameter);
            var second = factory.Create(secondParameter);

            Assert.That(receivedContainer, Is.SameAs(container));
            Assert.That(receivedParameter, Is.SameAs(secondParameter));
            Assert.That(callsCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Create_FromMethod_WhenContractHasConcreteResultType_ReturnsConcreteResult()
        {
            var container = new Container();
            container.BindFactory<Class, IProduct, ClassIProductFactory>()
                .To<Product>()
                .FromMethod((_, __) => new Product())
                .AsCached();
            var factory = container.Resolve<ClassIProductFactory>();

            var instance = factory.Create(new Class());

            Assert.That(instance, Is.TypeOf<Product>());
        }

        [Test]
        public void BindFactoryWithParameter_FromMethod_WhenMethodIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.BindFactory<Class, Product, ClassProductFactory>().FromMethod(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Create_FromMethod_WhenMethodReturnsNull_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.BindFactory<Class, Product, ClassProductFactory>()
                .FromMethod((_, __) => null)
                .AsCached();
            var factory = container.Resolve<ClassProductFactory>();

            Assert.That(
                () => factory.Create(new Class()),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
