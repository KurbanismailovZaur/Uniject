using NUnit.Framework;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public class ContainerResolveFromSubcontainerTests : ContainerResolveTestFixture
    {
        private class SubcontainerClass
        {
            public static int InstancesCount { get; set; }

            public SubcontainerClass() => InstancesCount++;
        }

        private class ClassWithParentDependency
        {
            public Class Dependency { get; }

            public ClassWithParentDependency(Class dependency)
            {
                Dependency = dependency;
            }
        }

        private class SubcontainerNonLazyClass
        {
            public static int InstancesCount { get; set; }

            public SubcontainerNonLazyClass() => InstancesCount++;
        }

        private class SubcontainerEntryPoint : IEntryPoint
        {
            public static int RunsCount { get; set; }

            public void Run() => RunsCount++;
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_AsTransient_ReturnsDifferentInstancesFromSubcontainer()
        {
            SubcontainerClass.InstancesCount = 0;

            var subcontainer = new Container();
            subcontainer.Bind<SubcontainerClass>().AsTransient();

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstance(subcontainer).AsTransient();

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_AsCached_WhenResolvedRepeatedly_ReturnsResolvedInstances()
        {
            SubcontainerClass.InstancesCount = 0;

            var subcontainer = new Container();
            subcontainer.Bind<SubcontainerClass>().AsTransient();

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstance(subcontainer).AsCached();

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_WhenSubcontainerNeedsParentDependency_ResolvesParentBinding()
        {
            var dependency = new Class();

            var subcontainer = new Container();
            subcontainer.Bind<ClassWithParentDependency>().AsTransient();

            var container = new Container();
            container.Bind<Class>().FromInstance(dependency);
            container.Bind<ClassWithParentDependency>().FromSubcontainerResolve().ByInstance(subcontainer).AsTransient();

            var resolved = container.Resolve<ClassWithParentDependency>();

            Assert.That(resolved.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_BuildsSubcontainerBeforeResolving()
        {
            SubcontainerClass.InstancesCount = 0;
            SubcontainerNonLazyClass.InstancesCount = 0;
            SubcontainerEntryPoint.RunsCount = 0;

            var subcontainer = new Container();
            subcontainer.Bind<SubcontainerNonLazyClass>().AsTransient().NonLazy();
            subcontainer.Bind<SubcontainerEntryPoint>().AsEntryPoint();
            subcontainer.Bind<SubcontainerClass>().AsTransient();

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstance(subcontainer).AsTransient();

            var resolved = container.Resolve<SubcontainerClass>();

            Assert.That(resolved, Is.Not.Null);
            Assert.That(subcontainer.IsBuilded, Is.True);
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(1));
            Assert.That(SubcontainerNonLazyClass.InstancesCount, Is.EqualTo(1));
            Assert.That(SubcontainerEntryPoint.RunsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_WhenContractHasConcreteType_ReturnsConcreteFromSubcontainer()
        {
            var subcontainer = new Container();
            subcontainer.Bind<ClassImplementedIInterface>().AsTransient();

            var container = new Container();
            container.Bind<IInterface>().To<ClassImplementedIInterface>()
                .FromSubcontainerResolve().ByInstance(subcontainer).AsTransient();

            var resolved = container.Resolve<IInterface>();

            Assert.That(resolved, Is.InstanceOf<ClassImplementedIInterface>());
        }
    }
}
