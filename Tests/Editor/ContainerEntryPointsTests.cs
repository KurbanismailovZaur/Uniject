using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject.Lifecycle;

namespace Uniject.Tests
{
    public class ContainerEntryPointsTests
    {
        private class EntryPointClass : IEntryPoint
        {
            public static int InstancesCount { get; set; }
            public static int RunsCount { get; set; }

            public EntryPointClass() => InstancesCount++;

            public void Run() => RunsCount++;
        }

        private class NotEntryPoint { }

        private static class EntryPointOrder
        {
            public static readonly List<string> Items = new();
            public static void Clear() => Items.Clear();
        }

        private class FirstEntryPoint : IEntryPoint
        {
            public void Run() => EntryPointOrder.Items.Add("First");
        }

        private class SecondEntryPoint : IEntryPoint
        {
            public void Run() => EntryPointOrder.Items.Add("Second");
        }

        private class DisposableEntryPoint : IEntryPoint, IDisposable
        {
            public int RunsCount { get; private set; }
            public int DisposesCount { get; private set; }

            public void Run() => RunsCount++;

            public void Dispose() => DisposesCount++;
        }

        private static class DisposableEntryPointOrder
        {
            public static readonly List<string> Items = new();

            public static void Clear() => Items.Clear();
        }

        private class FirstDisposableEntryPoint : IEntryPoint, IDisposable
        {
            public void Run() { }

            public void Dispose() => DisposableEntryPointOrder.Items.Add("First");
        }

        private class SecondDisposableEntryPoint : IEntryPoint, IDisposable
        {
            public void Run() { }

            public void Dispose() => DisposableEntryPointOrder.Items.Add("Second");
        }

        private static void ResolveNonLazyBindings(Container container)
        {
            var method = typeof(Container).GetMethod("ResolveNonLazyBindings", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                method.Invoke(container, null);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        private static void CallEntryPoints(Container container)
        {
            var method = typeof(Container).GetMethod("CallEntryPoints", BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                method.Invoke(container, null);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        [Test]
        public void CallEntryPoints_WhenBindingIsEntryPoint_CallsRun()
        {
            EntryPointClass.InstancesCount = 0;
            EntryPointClass.RunsCount = 0;

            var container = new Container();
            container.Bind<EntryPointClass>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointClass.InstancesCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.RunsCount, Is.EqualTo(1));
        }

        [Test]
        public void CallEntryPoints_WhenBindingIsOnlyNonLazy_DoesNotCallRun()
        {
            EntryPointClass.RunsCount = 0;

            var container = new Container();
            container.Bind<EntryPointClass>().NonLazy();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointClass.RunsCount, Is.EqualTo(0));
        }

        [Test]
        public void CallEntryPoints_WhenSeveralEntryPointsExist_CallsRunInRegistrationOrder()
        {
            EntryPointOrder.Clear();

            var container = new Container();
            container.Bind<FirstEntryPoint>().AsEntryPoint();
            container.Bind<SecondEntryPoint>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(EntryPointOrder.Items, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void Resolve_WhenBindingIsEntryPoint_ReturnsSameInstanceAfterNonLazyResolve()
        {
            EntryPointClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<EntryPointClass>().AsTransient().NonLazy().AsEntryPoint();

            ResolveNonLazyBindings(container);

            var first = container.Resolve<EntryPointClass>();
            var second = container.Resolve<EntryPointClass>();

            Assert.That(EntryPointClass.InstancesCount, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void AsEntryPoint_WhenConcreteTypeDoesNotImplementIEntryPoint_ThrowsInvalidOperationException()
        {
            var container = new Container();
            Assert.That(() => container.Bind<NotEntryPoint>().AsEntryPoint(), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Dispose_WhenEntryPointIsDisposable_DisposesEntryPoint()
        {
            var container = new Container();
            container.Bind<DisposableEntryPoint>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            var entryPoint = container.Resolve<DisposableEntryPoint>();

            CallEntryPoints(container);
            container.Dispose();

            Assert.That(entryPoint.DisposesCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WhenNoDisposableEntryPoints_DoesNotThrow()
        {
            var container = new Container();

            Assert.That(() => container.Dispose(), Throws.Nothing);
        }

        [Test]
        public void Dispose_WhenDisposableBindingIsNotEntryPoint_DoesNotDisposeIt()
        {
            var container = new Container();
            container.Bind<DisposableEntryPoint>().AsCached();

            var entryPoint = container.Resolve<DisposableEntryPoint>();

            container.Dispose();

            Assert.That(entryPoint.DisposesCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_WhenEntryPointIsBoundFromInstance_DisposesIt()
        {
            var entryPoint = new DisposableEntryPoint();

            var container = new Container();
            container.Bind<DisposableEntryPoint>().FromInstance(entryPoint).AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);
            container.Dispose();

            Assert.That(entryPoint.DisposesCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WhenSeveralDisposableEntryPointsExist_DisposesInReverseRegistrationOrder()
        {
            DisposableEntryPointOrder.Clear();

            var container = new Container();
            container.Bind<FirstDisposableEntryPoint>().AsEntryPoint();
            container.Bind<SecondDisposableEntryPoint>().AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);
            container.Dispose();

            Assert.That(DisposableEntryPointOrder.Items, Is.EqualTo(new[] { "Second", "First" }));
        }

        [Test]
        public void CallEntryPoints_WhenBindInstanceIsMarkedAsEntryPoint_CallsRun()
        {
            var entryPoint = new DisposableEntryPoint();

            var container = new Container();
            container.BindInstance(entryPoint).AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);

            Assert.That(entryPoint.RunsCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_WhenBindInstanceIsMarkedAsEntryPoint_DisposesIt()
        {
            var entryPoint = new DisposableEntryPoint();

            var container = new Container();
            container.BindInstance(entryPoint).AsEntryPoint();

            ResolveNonLazyBindings(container);
            CallEntryPoints(container);
            container.Dispose();

            Assert.That(entryPoint.DisposesCount, Is.EqualTo(1));
        }
    }
}
