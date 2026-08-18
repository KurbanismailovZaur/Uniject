using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Installers;
using Uniject.Lifecycle;

namespace Uniject.Tests
{
    public class ContainerChildContainerOwnershipTests
    {
        private sealed class DisposableResource : IDisposable
        {
            public int DisposeCallsCount { get; private set; }

            public DisposableResource() { }

            public void Dispose() => DisposeCallsCount++;
        }

        private sealed class DisposableInstaller : IInstaller
        {
            public DisposableInstaller() { }

            public void Install(Container container)
            {
                container.Bind<DisposableResource>()
                    .AsCached()
                    .DisposeWithContainer();
            }
        }

        private abstract class OrderedDisposable : IDisposable
        {
            private readonly string _name;
            private readonly ICollection<string> _disposeOrder;

            protected OrderedDisposable(string name, ICollection<string> disposeOrder)
            {
                _name = name;
                _disposeOrder = disposeOrder;
            }

            public void Dispose() => _disposeOrder.Add(_name);
        }

        private sealed class ChildOrderedDisposable : OrderedDisposable
        {
            public ChildOrderedDisposable(ICollection<string> disposeOrder)
                : base("child", disposeOrder) { }
        }

        private sealed class ParentOrderedDisposable : OrderedDisposable
        {
            public ParentOrderedDisposable(ICollection<string> disposeOrder)
                : base("parent", disposeOrder) { }
        }

        private sealed class FailingChildBinding { }
        private sealed class BuildFailure { }

        private sealed class ThrowingDisposable : IDisposable
        {
            public int DisposeCallsCount { get; private set; }

            public void Dispose()
            {
                DisposeCallsCount++;
                throw new InvalidOperationException("cleanup failed");
            }
        }

        private sealed class DisposableEntryPoint : IEntryPoint, IDisposable
        {
            public int DisposeCallsCount { get; private set; }

            public DisposableEntryPoint() { }

            public void Run() { }

            public void Dispose() => DisposeCallsCount++;
        }

        [Test]
        public void Dispose_ByMethodAsCached_DisposesOwnedChildResource()
        {
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                    child.Bind<DisposableResource>()
                        .AsCached()
                        .DisposeWithContainer())
                .AsCached();

            var resource = parent.Resolve<DisposableResource>();

            parent.Dispose();

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ByMethodAsTransient_DisposesEveryOwnedChildResourceOnce()
        {
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                    child.Bind<DisposableResource>()
                        .AsCached()
                        .DisposeWithContainer())
                .AsTransient();

            var first = parent.Resolve<DisposableResource>();
            var second = parent.Resolve<DisposableResource>();

            parent.Dispose();

            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(first.DisposeCallsCount, Is.EqualTo(1));
            Assert.That(second.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ByInstaller_DisposesOwnedChildResource()
        {
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByInstaller(new DisposableInstaller())
                .AsCached();

            var resource = parent.Resolve<DisposableResource>();

            parent.Dispose();

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ByGenericInstaller_DisposesOwnedChildResource()
        {
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByInstaller<DisposableInstaller>()
                .AsCached();

            var resource = parent.Resolve<DisposableResource>();

            parent.Dispose();

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WhenByMethodInstallFails_DisposesUnregisteredChildImmediately()
        {
            var resource = new DisposableResource();
            var parent = new Container();
            parent.Bind<FailingChildBinding>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                {
                    child.BindInstance(resource).DisposeWithContainer();
                    throw new InvalidOperationException("install failed");
                })
                .AsCached();

            Assert.That(
                () => parent.Resolve<FailingChildBinding>(),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));

            parent.Dispose();

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WhenOwnedChildBuildFails_DisposesAndUnregistersChildImmediately()
        {
            var resource = new DisposableResource();
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                {
                    child.BindInstance(resource).DisposeWithContainer();
                    child.Bind<BuildFailure>()
                        .FromMethod(_ => throw new InvalidOperationException("build failed"))
                        .AsCached()
                        .NonLazy();
                })
                .AsCached();

            Assert.That(
                () => parent.Resolve<DisposableResource>(),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));

            parent.Dispose();

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_WhenInstallAndCleanupFail_AggregatesBothErrorsInOrder()
        {
            var resource = new ThrowingDisposable();
            var parent = new Container();
            parent.Bind<FailingChildBinding>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                {
                    child.BindInstance(resource).DisposeWithContainer();
                    throw new InvalidOperationException("install failed");
                })
                .AsCached();

            var exception = Assert.Throws<AggregateException>(
                () => parent.Resolve<FailingChildBinding>());

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
            Assert.That(exception.InnerExceptions, Has.Count.EqualTo(2));
            Assert.That(exception.InnerExceptions[0].Message, Is.EqualTo("install failed"));
            Assert.That(exception.InnerExceptions[1].Message, Is.EqualTo("cleanup failed"));

            parent.Dispose();
        }

        [Test]
        public void Resolve_ByMethodAsCached_WhenFirstResolveFails_DisposesChildAndDoesNotCacheIt()
        {
            var resources = new List<DisposableResource>();
            var installCallsCount = 0;
            var parent = new Container();
            parent.Bind<FailingChildBinding>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                {
                    installCallsCount++;
                    var resource = new DisposableResource();
                    resources.Add(resource);
                    child.BindInstance(resource).DisposeWithContainer();
                    child.Bind<FailingChildBinding>()
                        .FromMethod(_ => throw new InvalidOperationException("resolve failed"))
                        .AsTransient();
                })
                .AsCached();

            Assert.That(
                () => parent.Resolve<FailingChildBinding>(),
                Throws.TypeOf<InvalidOperationException>());
            Assert.That(
                () => parent.Resolve<FailingChildBinding>(),
                Throws.TypeOf<InvalidOperationException>());

            Assert.That(installCallsCount, Is.EqualTo(2));
            Assert.That(resources, Has.Count.EqualTo(2));
            Assert.That(resources[0].DisposeCallsCount, Is.EqualTo(1));
            Assert.That(resources[1].DisposeCallsCount, Is.EqualTo(1));

            parent.Dispose();

            Assert.That(resources[0].DisposeCallsCount, Is.EqualTo(1));
            Assert.That(resources[1].DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_DisposesOwnedChildBeforeParentResource()
        {
            var disposeOrder = new List<string>();
            var parent = new Container();
            parent.Bind<ChildOrderedDisposable>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                    child.Bind<ChildOrderedDisposable>()
                        .FromMethod(_ => new ChildOrderedDisposable(disposeOrder))
                        .AsCached()
                        .DisposeWithContainer())
                .AsCached();
            parent.Bind<ParentOrderedDisposable>()
                .FromMethod(_ => new ParentOrderedDisposable(disposeOrder))
                .AsCached()
                .DisposeWithContainer();

            parent.Resolve<ChildOrderedDisposable>();
            parent.Resolve<ParentOrderedDisposable>();

            parent.Dispose();

            Assert.That(disposeOrder, Is.EqualTo(new[] { "child", "parent" }));
        }

        [Test]
        public void Dispose_WhenOwnedChildAndParentResultBothOwnInstance_DisposesItOnce()
        {
            var parent = new Container();
            parent.Bind<DisposableEntryPoint>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                    child.Bind<DisposableEntryPoint>()
                        .AsCached()
                        .DisposeWithContainer())
                .AsEntryPoint()
                .DisposeWithContainer();

            parent.Build();
            var entryPoint = parent.Resolve<DisposableEntryPoint>();

            parent.Dispose();

            Assert.That(entryPoint.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WhenDuplicatedOwnedChildWasDisposedFirst_ParentDoesNotDisposeInstanceAgain()
        {
            var parent = new Container();
            Container ownedChild = null;
            parent.Bind<DisposableEntryPoint>()
                .FromSubcontainerResolve()
                .ByMethod(child =>
                {
                    ownedChild = child;
                    child.Bind<DisposableEntryPoint>()
                        .AsCached()
                        .DisposeWithContainer();
                })
                .AsEntryPoint()
                .DisposeWithContainer();

            parent.Build();
            var entryPoint = parent.Resolve<DisposableEntryPoint>();

            ownedChild.Dispose();
            parent.Dispose();

            Assert.That(entryPoint.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ByInstance_DoesNotDisposeBorrowedChildContainer()
        {
            var child = new Container();
            child.Bind<DisposableResource>()
                .AsCached()
                .DisposeWithContainer();
            var parent = new Container();
            parent.Bind<DisposableResource>()
                .FromSubcontainerResolve()
                .ByInstance(child)
                .AsCached();
            var resource = parent.Resolve<DisposableResource>();

            try
            {
                parent.Dispose();

                Assert.That(resource.DisposeCallsCount, Is.Zero);
            }
            finally
            {
                child.Dispose();
            }

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ManuallyParentedContainer_DoesNotDisposeChildContainer()
        {
            var parent = new Container();
            var child = new Container(parent);
            child.Bind<DisposableResource>()
                .AsCached()
                .DisposeWithContainer();
            var resource = child.Resolve<DisposableResource>();

            try
            {
                parent.Dispose();

                Assert.That(resource.DisposeCallsCount, Is.Zero);
            }
            finally
            {
                child.Dispose();
            }

            Assert.That(resource.DisposeCallsCount, Is.EqualTo(1));
        }
    }
}
