using System.Reflection;
using NUnit.Framework;
using Uniject;

namespace Uniject.Tests
{
    public class ContainerCircularDependencyTests
    {
        private class ConstructorCircularA
        {
            public ConstructorCircularA(ConstructorCircularB dependency) { }
        }

        private class ConstructorCircularB
        {
            public ConstructorCircularB(ConstructorCircularA dependency) { }
        }

        private interface IFromResolveCircularDependency
        {
        }

        private class FromResolveCircularDependency : IFromResolveCircularDependency
        {
            public FromResolveCircularDependency(IFromResolveCircularDependency dependency) { }
        }

        private interface IFromResolveGetterCircularDependency
        {
        }

        private class FromResolveGetterCircularSource
        {
            public IFromResolveGetterCircularDependency Dependency { get; }

            public FromResolveGetterCircularSource(IFromResolveGetterCircularDependency dependency)
            {
                Dependency = dependency;
            }
        }

        private static void AssertCircularDependency(TestDelegate action)
        {
            Assert.That(action, Throws.Exception.With.Message.Contains("Circular dependency detected"));
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
        public void Resolve_FromConstructor_WhenDependenciesAreCircular_ThrowsException()
        {
            var container = new Container();
            container.Bind<ConstructorCircularA>();
            container.Bind<ConstructorCircularB>();

            AssertCircularDependency(() => container.Resolve<ConstructorCircularA>());
        }

        [Test]
        public void Resolve_FromResolve_WhenConcreteDependencyResolvesContractAgain_ThrowsException()
        {
            var container = new Container();
            container.Bind<IFromResolveCircularDependency>().To<FromResolveCircularDependency>().FromResolve();
            container.Bind<FromResolveCircularDependency>();

            AssertCircularDependency(() => container.Resolve<IFromResolveCircularDependency>());
        }

        [Test]
        public void Resolve_FromResolveGetter_WhenSourceDependsOnContract_ThrowsException()
        {
            var container = new Container();
            container.Bind<FromResolveGetterCircularSource>();
            container.Bind<IFromResolveGetterCircularDependency>()
                .FromResolveGetter<FromResolveGetterCircularSource>(source => source.Dependency);

            AssertCircularDependency(() => container.Resolve<IFromResolveGetterCircularDependency>());
        }

        [Test]
        public void ResolveNonLazyBindings_WhenNonLazyBindingHasCircularDependencies_ThrowsException()
        {
            var container = new Container();
            container.Bind<ConstructorCircularA>().AsTransient().NonLazy();
            container.Bind<ConstructorCircularB>();

            AssertCircularDependency(() => ResolveNonLazyBindings(container));
        }
    }
}
