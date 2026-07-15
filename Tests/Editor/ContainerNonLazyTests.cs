using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject;

namespace Uniject.Tests
{
    public class ContainerNonLazyTests
    {
        private class NonLazyTransientClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyTransientClass() => InstancesCount++;
        }

        private class NonLazyCachedClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyCachedClass() => InstancesCount++;
        }

        private class NonLazyShortcutClass
        {
            public static int InstancesCount { get; set; }
            public NonLazyShortcutClass() => InstancesCount++;
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

        private class LazyClass
        {
            public static int InstancesCount { get; set; }
            public LazyClass() => InstancesCount++;
        }

        private static class NonLazyOrder
        {
            public static readonly List<string> Items = new();

            public static void Clear() => Items.Clear();
        }

        private class FirstNonLazyClass
        {
            public FirstNonLazyClass() => NonLazyOrder.Items.Add("First");
        }

        private class SecondNonLazyClass
        {
            public SecondNonLazyClass() => NonLazyOrder.Items.Add("Second");
        }

        private class FailsOnFirstCreationClass
        {
            public static int CreationAttemptsCount { get; set; }

            public FailsOnFirstCreationClass()
            {
                CreationAttemptsCount++;

                if (CreationAttemptsCount == 1)
                    throw new System.Exception("First creation failed.");
            }
        }

        [Test]
        public void ResolveNonLazyBindings_WhenTransientBindingIsNonLazy_CreatesInstanceBeforeResolve()
        {
            NonLazyTransientClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyTransientClass>().AsTransient().NonLazy();

            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(0));

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));

            var first = container.Resolve<NonLazyTransientClass>();
            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));

            var second = container.Resolve<NonLazyTransientClass>();
            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(2));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenCachedBindingIsNonLazy_CreatesAndCachesInstance()
        {
            NonLazyCachedClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyCachedClass>().AsCached().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyCachedClass.InstancesCount, Is.EqualTo(1));

            var first = container.Resolve<NonLazyCachedClass>();
            var second = container.Resolve<NonLazyCachedClass>();

            Assert.That(NonLazyCachedClass.InstancesCount, Is.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenShortcutNonLazyIsUsed_CreatesTransientInstance()
        {
            NonLazyShortcutClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyShortcutClass>().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyShortcutClass.InstancesCount, Is.EqualTo(1));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenLazyAndNonLazyBindingsExist_ResolvesOnlyNonLazyBindings()
        {
            LazyClass.InstancesCount = 0;
            NonLazyTransientClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<LazyClass>().AsTransient();
            container.Bind<NonLazyTransientClass>().AsTransient().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(LazyClass.InstancesCount, Is.EqualTo(0));
            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenSeveralBindingsAreNonLazy_ResolvesThemInRegistrationOrder()
        {
            NonLazyOrder.Clear();

            var container = new Container();
            container.Bind<FirstNonLazyClass>().AsTransient().NonLazy();
            container.Bind<SecondNonLazyClass>().AsTransient().NonLazy();

            ResolveNonLazyBindings(container);

            Assert.That(NonLazyOrder.Items, Is.EqualTo(new[] { "First", "Second" }));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenTransientWasResolvedBeforeBuild_DoesNotCreateExtraInstance()
        {
            NonLazyTransientClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<NonLazyTransientClass>().AsTransient().NonLazy();

            container.Resolve<NonLazyTransientClass>();
            ResolveNonLazyBindings(container);

            Assert.That(NonLazyTransientClass.InstancesCount, Is.EqualTo(1));
        }

        [Test]
        public void ResolveNonLazyBindings_WhenPreviousCreationFailed_DoesNotRetryCreation()
        {
            FailsOnFirstCreationClass.CreationAttemptsCount = 0;

            var container = new Container();
            container.Bind<FailsOnFirstCreationClass>().AsTransient().NonLazy();

            Assert.That(
                () => container.Resolve<FailsOnFirstCreationClass>(),
                Throws.Exception);

            Assert.That(
                () => ResolveNonLazyBindings(container),
                Throws.Nothing);

            Assert.That(FailsOnFirstCreationClass.CreationAttemptsCount, Is.EqualTo(1));
        }
    }
}
