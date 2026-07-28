using System;
using NUnit.Framework;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public class ContainerResolveFromMethodTests : ContainerResolveTestFixture
    {
        [Test]
        public void Resolve_FromMethod_InvokesMethodWithContainerForEveryTransientResolve()
        {
            var container = new Container();
            var callsCount = 0;
            var receivedContainer = default(Container);
            container.Bind<Class>().FromMethod(context =>
            {
                callsCount++;
                receivedContainer = context.Container;
                return new Class();
            }).AsTransient();

            var first = container.Resolve<Class>();
            var second = container.Resolve<Class>();

            Assert.That(receivedContainer, Is.SameAs(container));
            Assert.That(callsCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_FromMethod_WhenContractHasConcreteType_ReturnsConcreteResult()
        {
            var expected = new ClassImplementedIInterface();
            var container = new Container();
            container.Bind<IInterface>()
                .To<ClassImplementedIInterface>()
                .FromMethod(_ => expected)
                .AsTransient();

            var instance = container.Resolve<IInterface>();

            Assert.That(instance, Is.SameAs(expected));
        }

        [Test]
        public void Bind_FromMethod_WhenMethodIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<Class>().FromMethod((Func<InjectContext, Class>)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_FromMethod_WhenMethodReturnsNull_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<Class>().FromMethod(_ => null).AsTransient();

            Assert.That(
                () => container.Resolve<Class>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_FromMethod_WhenNonGenericBindingReturnsIncompatibleType_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind(typeof(IInterface)).FromMethod(_ => new Class()).AsTransient();

            Assert.That(
                () => container.Resolve<IInterface>(),
                Throws.TypeOf<InvalidOperationException>());
        }
    }
}
