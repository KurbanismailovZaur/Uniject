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
    }
}
