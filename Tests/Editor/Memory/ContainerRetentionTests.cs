using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Lifecycle;

namespace Uniject.Tests.Memory
{
    [Category("MemoryRetention")]
    public class ContainerRetentionTests
    {
        public enum ChildFailure { Install, Build, Resolve }
        private interface IFirst { }
        private interface ISecond { }
        private sealed class DisposeCounter { public int Count; }

        private sealed class Payload : IDisposable, IFirst, ISecond, IEntryPoint
        {
            public DisposeCounter Counter;
            public bool ThrowOnDispose;
            public Payload() { }
            public void Run() { }
            public void Dispose()
            {
                if (Counter != null) Counter.Count++;
                if (ThrowOnDispose) throw new InvalidOperationException("Expected disposal failure.");
            }
        }

        private sealed class InjectionTarget
        {
            public bool ThrowOnInject;
            [Inject]
            public void Construct(Container container)
            {
                if (ThrowOnInject) throw new InvalidOperationException("Expected injection failure.");
            }
        }

        private sealed class FailingBinding { }

        [TestCase(false)]
        [TestCase(true)]
        public void Dispose_RootContainer_DoesNotRetainCachedInstance(bool disposeWithContainer)
        {
            using var container = new Container();
            var reference = BindAndResolveCached(container, disposeWithContainer);
            MemoryRetentionAssert.Retained(container, reference);

            container.Dispose();

            MemoryRetentionAssert.Collected(container, reference);
        }

        [Test]
        public void Dispose_DoesNotRetainFromInstanceOrMethodClosure()
        {
            using var container = new Container();
            var references = BindInstanceAndClosure(container);

            container.Dispose();

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Resolve_TransientInstancesAreNotRetainedByLiveContainer()
        {
            using var container = new Container();
            container.Bind<Payload>().AsTransient();
            var references = new WeakReference[32];
            for (var i = 0; i < references.Length; i++)
                references[i] = ResolveWeak(container);

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Build_DoesNotRetainDrainedInjectionQueueTargets()
        {
            using var container = new Container();
            var references = QueueTargets(container);
            MemoryRetentionAssert.Retained(container, references);

            container.Build();

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Dispose_DoesNotRetainUnprocessedInjectionQueueTargets()
        {
            using var container = new Container();
            var references = QueueTargets(container);

            container.Dispose();

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Dispose_AfterInjectionFails_ReleasesFailedAndPendingTargets()
        {
            using var container = new Container();
            var references = QueueTargets(container, true);
            FailBuild(container);

            container.Dispose();

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Dispose_SharedInstanceUnderMultipleContracts_ReleasesItAfterOneDispose()
        {
            using var container = new Container();
            var counter = new DisposeCounter();
            var reference = BindShared(container, counter);

            container.Dispose();

            Assert.That(counter.Count, Is.EqualTo(1));
            MemoryRetentionAssert.Collected(container, reference);
        }

        [Test]
        public void Dispose_WhenResourceThrows_ReleasesAllResources()
        {
            using var container = new Container();
            var references = BindThrowingResources(container);
            DisposeExpectingFailure(container);

            MemoryRetentionAssert.Collected(container, references);
        }

        [Test]
        public void Dispose_DoesNotRetainBuiltEntryPoint()
        {
            using var container = new Container();
            container.Bind<Payload>().AsCached().NonLazy().AsEntryPoint().DisposeWithContainer();
            container.Build();
            var reference = ResolveWeak(container);

            container.Dispose();

            MemoryRetentionAssert.Collected(container, reference);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Dispose_ParentReleasesOwnedSubcontainerAndResource(bool cached)
        {
            using var parent = new Container();
            var references = ResolveOwnedChildren(parent, cached);
            MemoryRetentionAssert.Retained(parent, references);

            parent.Dispose();

            MemoryRetentionAssert.Collected(parent, references);
        }

        [Test]
        public void Dispose_OwnedChildFirst_ReleasesDisposalHistoryWhenParentDisposes()
        {
            using var parent = new Container();
            var reference = ResolveOwnedChild(parent, out var child);
            child.Dispose();
            // The child deliberately keeps disposal history until its owner finishes cleanup.
            MemoryRetentionAssert.Retained(new object[] { parent, child }, reference);

            parent.Dispose();

            MemoryRetentionAssert.Collected(new object[] { parent, child }, reference);
        }

        [Test]
        public void Dispose_BorrowedSubcontainerKeepsResourceUntilItsOwnDisposal()
        {
            using var parent = new Container();
            using var child = new Container();
            var reference = BindAndResolveCached(child, true);
            parent.Bind<Payload>().FromSubcontainerResolve().ByInstance(child).AsCached();
            ResolveWeak(parent);

            parent.Dispose();

            MemoryRetentionAssert.Retained(new object[] { parent, child }, reference);
            child.Dispose();
            MemoryRetentionAssert.Collected(new object[] { parent, child }, reference);
        }

        [Test]
        public void Resolve_FailedOwnedSubcontainerDoesNotRetainChildOrResource([Values] ChildFailure failure)
        {
            using var parent = new Container();
            var references = ResolveFailingChild(parent, failure);

            MemoryRetentionAssert.Collected(parent, references);
        }

        [Test]
        public void RepeatedCreateBuildDispose_DoesNotRetainEarlierObjectGraphs()
        {
            var disposedContainers = new Container[16];
            var references = new WeakReference[disposedContainers.Length];
            try
            {
                for (var i = 0; i < disposedContainers.Length; i++)
                    references[i] = CreateBuildDispose(out disposedContainers[i]);

                MemoryRetentionAssert.Collected(disposedContainers, references);
            }
            finally
            {
                foreach (var container in disposedContainers)
                    container?.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference BindAndResolveCached(Container container, bool disposeWithContainer)
        {
            var builder = container.Bind<Payload>().AsCached();
            if (disposeWithContainer) builder.DisposeWithContainer();
            return new WeakReference(container.Resolve<Payload>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] BindInstanceAndClosure(Container container)
        {
            var instance = new Payload();
            var captured = new Payload();
            container.Bind<IFirst>().FromInstance(instance).AsCached();
            container.Bind<ISecond>().FromMethod(_ => captured).AsCached();
            return new[] { new WeakReference(instance), new WeakReference(captured) };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ResolveWeak(Container container) => new(container.Resolve<Payload>());

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] QueueTargets(Container container, bool failFirst = false)
        {
            var first = new InjectionTarget { ThrowOnInject = failFirst };
            var last = new InjectionTarget();
            container.AddToInjectionQueue(first);
            container.AddToInjectionQueue(last);
            return new[] { new WeakReference(first), new WeakReference(last) };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FailBuild(Container container)
        {
            Assert.Throws<TargetInvocationException>(() => container.Build());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference BindShared(Container container, DisposeCounter counter)
        {
            var payload = new Payload { Counter = counter };
            container.Bind<IFirst>().FromInstance(payload).AsCached().DisposeWithContainer();
            container.Bind<ISecond>().FromInstance(payload).AsCached().DisposeWithContainer();
            return new WeakReference(payload);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] BindThrowingResources(Container container)
        {
            var first = new Payload();
            var second = new Payload { ThrowOnDispose = true };
            container.Bind<IFirst>().FromInstance(first).AsCached().DisposeWithContainer();
            container.Bind<ISecond>().FromInstance(second).AsCached().DisposeWithContainer();
            return new[] { new WeakReference(first), new WeakReference(second) };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DisposeExpectingFailure(Container container)
        {
            Assert.Throws<AggregateException>(() => container.Dispose());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] ResolveOwnedChildren(Container parent, bool cached)
        {
            var references = new WeakReference[cached ? 2 : 8];
            var nextChild = 0;
            var builder = parent.Bind<Payload>().FromSubcontainerResolve().ByMethod(child =>
            {
                references[nextChild++] = new WeakReference(child);
                child.Bind<Payload>().AsCached().DisposeWithContainer();
            });
            if (cached) builder.AsCached();
            else builder.AsTransient();

            var count = references.Length / 2;
            for (var i = 0; i < count; i++)
                references[count + i] = ResolveWeak(parent);
            return references;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ResolveOwnedChild(Container parent, out Container ownedChild)
        {
            Container child = null;
            parent.Bind<Payload>().FromSubcontainerResolve().ByMethod(created =>
            {
                child = created;
                created.Bind<Payload>().AsCached().DisposeWithContainer();
            }).AsTransient();
            var reference = ResolveWeak(parent);
            ownedChild = child;
            return reference;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] ResolveFailingChild(Container parent, ChildFailure failure)
        {
            var references = new WeakReference[2];
            parent.Bind<FailingBinding>().FromSubcontainerResolve().ByMethod(child =>
            {
                var resource = new Payload();
                references[0] = new WeakReference(child);
                references[1] = new WeakReference(resource);
                child.BindInstance(resource).DisposeWithContainer();
                if (failure == ChildFailure.Install)
                    throw new InvalidOperationException("Expected installation failure.");

                var binding = child.Bind<FailingBinding>()
                    .FromMethod(_ => throw new InvalidOperationException("Expected creation failure."))
                    .AsCached();
                if (failure == ChildFailure.Build) binding.NonLazy();
            }).AsCached();
            Assert.Throws<InvalidOperationException>(() => parent.Resolve<FailingBinding>());
            return references;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateBuildDispose(out Container container)
        {
            container = new Container();
            var reference = BindAndResolveCached(container, true);
            container.Build();
            container.Dispose();
            return reference;
        }
    }
}
