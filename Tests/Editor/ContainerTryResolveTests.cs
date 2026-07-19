using NUnit.Framework;
using Uniject.Exceptions;
using Uniject.Installers;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerTryResolveTests
    {
        private sealed class CircularA
        {
            public CircularA(CircularB dependency) { }
        }

        private sealed class CircularB
        {
            public CircularB(CircularA dependency) { }
        }

        private sealed class ClassWithMissingDependency
        {
            public ClassWithMissingDependency(MissingDependency dependency) { }
        }

        private sealed class MissingDependency
        {
        }

        [Test]
        public void Resolve_WhenBindingIsMissing_ThrowsNoBindingFoundException()
        {
            var container = new Container();

            Assert.That(
                () => container.Resolve<Class>(),
                Throws.TypeOf<NoBindingFoundException>());
        }

        [Test]
        public void TryResolve_WhenBindingIsMissing_ReturnsDefaultAndFalse()
        {
            var container = new Container();

            var (instance, resolved) = container.TryResolve<Class>();

            Assert.That(resolved, Is.False);
            Assert.That(instance, Is.Null);
        }

        [Test]
        public void TryResolve_WhenBindingExists_ReturnsResolvedInstance()
        {
            var container = new Container();
            container.Bind<Class>().AsCached();

            var (instance, resolved) = container.TryResolve<Class>();

            Assert.That(resolved, Is.True);
            Assert.That(instance, Is.SameAs(container.Resolve<Class>()));
        }

        [Test]
        public void TryResolve_WhenDefaultValueTypeBindingExists_ReturnsDefaultAndTrue()
        {
            var container = new Container();
            container.BindInstance(0);

            var (instance, resolved) = container.TryResolve<int>();

            Assert.That(resolved, Is.True);
            Assert.That(instance, Is.Zero);
        }

        [Test]
        public void TryResolve_WhenNullableValueTypeBindingExists_ReturnsValueAndTrue()
        {
            var container = new Container();
            container.BindInstance(0);
            container.Bind<int?>().AsCached();

            var (instance, resolved) = container.TryResolve<int?>();

            Assert.That(resolved, Is.True);
            Assert.That(instance, Is.Zero);
        }

        [Test]
        public void TryResolve_WhenNonNullableValueTypeBindingIsMissing_ReturnsDefaultAndFalse()
        {
            var container = new Container();

            var (instance, resolved) = container.TryResolve<int>();

            Assert.That(resolved, Is.False);
            Assert.That(instance, Is.EqualTo(default(int)));
        }

        [Test]
        public void TryResolve_WhenNullableValueTypeBindingIsMissing_ReturnsNullAndFalse()
        {
            var container = new Container();

            var (instance, resolved) = container.TryResolve<int?>();

            Assert.That(resolved, Is.False);
            Assert.That(instance, Is.Null);
        }

        [Test]
        public void TryResolve_WhenDependencyBindingIsMissing_ReturnsDefaultAndFalse()
        {
            var container = new Container();
            container.Bind<ClassWithMissingDependency>();

            var (instance, resolved) = container.TryResolve<ClassWithMissingDependency>();

            Assert.That(resolved, Is.False);
            Assert.That(instance, Is.Null);
        }

        [Test]
        public void TryResolveType_WhenBindingIsMissing_ReturnsNull()
        {
            var container = new Container();

            Assert.That(container.TryResolve(typeof(Class)), Is.Null);
        }

        [Test]
        public void TryResolveType_WhenBindingExists_ReturnsResolvedInstance()
        {
            var container = new Container();
            var expected = new Class();
            container.BindInstance(expected);

            Assert.That(container.TryResolve(typeof(Class)), Is.SameAs(expected));
        }

        [Test]
        public void TryResolve_WhenResolutionFailsForAnotherReason_DoesNotSuppressException()
        {
            var container = new Container();
            container.Bind<CircularA>();
            container.Bind<CircularB>();

            Assert.That(
                () => container.TryResolve<CircularA>(),
                Throws.Exception.With.Message.Contains("Circular dependency detected"));
        }

        [Test]
        public void SceneLoaderInstaller_Install_BindsCachedSceneLoader()
        {
            var gameObject = new GameObject("SceneLoaderInstaller");
            var container = new Container();

            try
            {
                var installer = gameObject.AddComponent<SceneLoaderInstaller>();

                installer.Install(container);

                var first = container.Resolve<SceneLoader>();
                var second = container.Resolve<SceneLoader>();

                Assert.That(first, Is.Not.Null);
                Assert.That(second, Is.SameAs(first));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
