using System;
using NUnit.Framework;
using Uniject.Exceptions;

namespace Uniject.Tests
{
    public class ContainerResolveFromResolveGetterTests : ContainerResolveTestFixture
    {
        private interface IService
        {
        }

        private class Service : IService
        {
        }

        private class AlternativeService : IService
        {
        }

        private interface IServiceSource
        {
            IService Service { get; }
            IService CreateService();
        }

        private class ServiceSource : IServiceSource
        {
            public IService Service { get; }

            public ServiceSource(IService service)
            {
                Service = service;
            }

            public IService CreateService() => new Service();
        }

        [Test]
        public void Resolve_FromResolveGetter_ResolvesInterfaceSourceAndPassesItToGetter()
        {
            var expected = new Service();
            IServiceSource source = new ServiceSource(expected);
            var receivedSource = default(IServiceSource);
            var container = new Container();
            container.Bind<IServiceSource>().FromInstance(source);
            container.Bind<IService>().FromResolveGetter<IServiceSource>(resolvedSource =>
            {
                receivedSource = resolvedSource;
                return resolvedSource.Service;
            }).AsTransient();

            var resolved = container.Resolve<IService>();

            Assert.That(receivedSource, Is.SameAs(source));
            Assert.That(resolved, Is.SameAs(expected));
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenSourceIsBoundAfterTarget_ResolvesResult()
        {
            var expected = new Service();
            IServiceSource source = new ServiceSource(expected);
            var container = new Container();
            container.Bind<IService>()
                .FromResolveGetter<IServiceSource>(resolvedSource => resolvedSource.Service)
                .AsTransient();
            container.Bind<IServiceSource>().FromInstance(source);

            var resolved = container.Resolve<IService>();

            Assert.That(resolved, Is.SameAs(expected));
        }

        [Test]
        public void Bind_FromResolveGetter_WhenBuilderWasUsedForTo_ResetsConcreteTypeToContractType()
        {
            var expected = new AlternativeService();
            IServiceSource source = new ServiceSource(expected);
            var container = new Container();
            var builder = container.Bind<IService>();
            builder.To<Service>();
            builder.FromResolveGetter<IServiceSource>(resolvedSource => resolvedSource.Service)
                .AsTransient();
            container.Bind<IServiceSource>().FromInstance(source);

            var resolved = container.Resolve<IService>();

            Assert.That(resolved, Is.SameAs(expected));
        }

        [Test]
        public void Resolve_FromResolveGetter_AsTransientWithCachedSource_InvokesGetterForEveryResolve()
        {
            var sourceCreationsCount = 0;
            var getterCallsCount = 0;
            var container = new Container();
            container.Bind<IServiceSource>().FromMethod(_ =>
            {
                sourceCreationsCount++;
                return new ServiceSource(new Service());
            }).AsCached();
            container.Bind<IService>().FromResolveGetter<IServiceSource>(source =>
            {
                getterCallsCount++;
                return source.CreateService();
            }).AsTransient();

            var first = container.Resolve<IService>();
            var second = container.Resolve<IService>();

            Assert.That(sourceCreationsCount, Is.EqualTo(1));
            Assert.That(getterCallsCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Resolve_FromResolveGetter_AsCachedWithTransientSource_InvokesGetterOnlyOnce()
        {
            var sourceCreationsCount = 0;
            var getterCallsCount = 0;
            var container = new Container();
            container.Bind<IServiceSource>().FromMethod(_ =>
            {
                sourceCreationsCount++;
                return new ServiceSource(new Service());
            }).AsTransient();
            container.Bind<IService>().FromResolveGetter<IServiceSource>(source =>
            {
                getterCallsCount++;
                return source.Service;
            }).AsCached();

            var first = container.Resolve<IService>();
            var second = container.Resolve<IService>();

            Assert.That(sourceCreationsCount, Is.EqualTo(1));
            Assert.That(getterCallsCount, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void Bind_FromResolveGetter_WhenGetterIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IService>()
                    .FromResolveGetter<IServiceSource>((Func<IServiceSource, IService>)null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Bind_FromResolveGetter_WhenSourceTypeEqualsContractType_ThrowsArgumentException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<IService>()
                    .FromResolveGetter<IService>(service => service),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenGetterReturnsNull_ThrowsInvalidOperationException()
        {
            var container = new Container();
            container.Bind<IServiceSource>()
                .FromInstance(new ServiceSource(new Service()));
            container.Bind<IService>()
                .FromResolveGetter<IServiceSource>(_ => null)
                .AsTransient();

            Assert.That(
                () => container.Resolve<IService>(),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenSourceIsNotBound_DoesNotInvokeGetter()
        {
            var getterCallsCount = 0;
            var container = new Container();
            container.Bind<IService>().FromResolveGetter<IServiceSource>(_ =>
            {
                getterCallsCount++;
                return new Service();
            }).AsTransient();

            Assert.That(
                () => container.Resolve<IService>(),
                Throws.TypeOf<NoBindingFoundException>());
            Assert.That(getterCallsCount, Is.Zero);
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenSourceIsValueType_PassesValueToGetter()
        {
            const int expectedSource = 42;
            var receivedSource = 0;
            var container = new Container();
            container.BindInstance(expectedSource);
            container.Bind<string>().FromResolveGetter<int>(source =>
            {
                receivedSource = source;
                return source.ToString();
            }).AsTransient();

            var resolved = container.Resolve<string>();

            Assert.That(receivedSource, Is.EqualTo(expectedSource));
            Assert.That(resolved, Is.EqualTo(expectedSource.ToString()));
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenBindingComesFromParent_UsesParentSource()
        {
            var parentService = new Service();
            var childService = new Service();
            var parent = new Container();
            parent.Bind<IServiceSource>()
                .FromInstance(new ServiceSource(parentService));
            parent.Bind<IService>()
                .FromResolveGetter<IServiceSource>(source => source.Service)
                .AsTransient();
            var child = new Container(parent);
            child.Bind<IServiceSource>()
                .FromInstance(new ServiceSource(childService));

            var resolved = child.Resolve<IService>();

            Assert.That(resolved, Is.SameAs(parentService));
            Assert.That(resolved, Is.Not.SameAs(childService));
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenGetterThrows_PropagatesExceptionAndCachedBindingRetries()
        {
            var expectedResult = new Service();
            var expectedException = new InvalidOperationException("Getter failed.");
            var getterCallsCount = 0;
            var container = new Container();
            container.Bind<IServiceSource>()
                .FromInstance(new ServiceSource(expectedResult));
            container.Bind<IService>().FromResolveGetter<IServiceSource>(source =>
            {
                getterCallsCount++;

                if (getterCallsCount == 1)
                    throw expectedException;

                return source.Service;
            }).AsCached();

            var actualException = Assert.Throws<InvalidOperationException>(
                () => container.Resolve<IService>());
            var first = container.Resolve<IService>();
            var second = container.Resolve<IService>();

            Assert.That(actualException, Is.SameAs(expectedException));
            Assert.That(getterCallsCount, Is.EqualTo(2));
            Assert.That(first, Is.SameAs(expectedResult));
            Assert.That(second, Is.SameAs(first));
        }
    }
}
