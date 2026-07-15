using NUnit.Framework;
using Uniject.Exceptions;
using Uniject.Tests.Fixtures;

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

        [Test]
        public void Resolve_WhenBindingIsMissing_ThrowsNoBindingFoundException()
        {
            var container = new Container();

            Assert.That(
                () => container.Resolve<Class>(),
                Throws.TypeOf<NoBindingFoundException>());
        }

        [Test]
        public void TryResolve_WhenBindingIsMissing_ReturnsNull()
        {
            var container = new Container();

            Assert.That(container.TryResolve<Class>(), Is.Null);
        }

        [Test]
        public void TryResolve_WhenBindingExists_ReturnsResolvedInstance()
        {
            var container = new Container();
            container.Bind<Class>().AsCached();

            Assert.That(container.TryResolve<Class>(), Is.SameAs(container.Resolve<Class>()));
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
        public void Constructor_BindsCachedSceneLoader()
        {
            var container = new Container();

            var first = container.Resolve<SceneLoader>();
            var second = container.Resolve<SceneLoader>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }
    }
}
