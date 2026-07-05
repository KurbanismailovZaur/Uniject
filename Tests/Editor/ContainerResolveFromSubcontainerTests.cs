using System;
using NUnit.Framework;
using Uniject.Installers;
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

        private class CountingInstaller : IInstaller
        {
            public int InstallCallsCount { get; private set; }

            public CountingInstaller() { }

            public void Install(Container container)
            {
                InstallCallsCount++;
                container.Bind<SubcontainerClass>().AsTransient();
            }
        }

        private class StaticCountingInstaller : IInstaller
        {
            public static int InstallCallsCount { get; set; }

            public StaticCountingInstaller() { }

            public void Install(Container container)
            {
                InstallCallsCount++;
                container.Bind<SubcontainerClass>().AsTransient();
            }
        }

        private class ParentDependencyInstaller : IInstaller
        {
            public ParentDependencyInstaller() { }

            public void Install(Container container)
            {
                container.Bind<ClassWithParentDependency>().AsTransient();
            }
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
        public void Resolve_FromSubcontainerResolve_ByMethod_WhenScopeIsNotSpecified_ReusesSubcontainer()
        {
            var installCallsCount = 0;
            SubcontainerClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByMethod(subcontainer =>
            {
                installCallsCount++;
                subcontainer.Bind<SubcontainerClass>().AsTransient();
            });

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
            Assert.That(installCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_ByMethod_AsTransient_CreatesSubcontainerForEveryResolve()
        {
            var installCallsCount = 0;
            SubcontainerClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByMethod(subcontainer =>
            {
                installCallsCount++;
                subcontainer.Bind<SubcontainerClass>().AsTransient();
            }).AsTransient();

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
            Assert.That(installCallsCount, Is.EqualTo(2));
        }

        [Test]
        public void ByInstance_WhenInstanceIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstance(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ByMethod_WhenInstallMethodIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByMethod(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ByInstaller_WhenInstallerIsNull_ThrowsArgumentNullException()
        {
            var container = new Container();

            Assert.That(
                () => container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstaller<CountingInstaller>(null),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_ByInstaller_WhenScopeIsNotSpecified_ReusesSubcontainer()
        {
            SubcontainerClass.InstancesCount = 0;
            var installer = new CountingInstaller();

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstaller(installer);

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
            Assert.That(installer.InstallCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_ByInstaller_AsTransient_CreatesSubcontainerForEveryResolve()
        {
            SubcontainerClass.InstancesCount = 0;
            var installer = new CountingInstaller();

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstaller(installer).AsTransient();

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
            Assert.That(installer.InstallCallsCount, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_ByInstallerGeneric_CreatesInstallerAndInstallsSubcontainer()
        {
            StaticCountingInstaller.InstallCallsCount = 0;
            SubcontainerClass.InstancesCount = 0;

            var container = new Container();
            container.Bind<SubcontainerClass>().FromSubcontainerResolve().ByInstaller<StaticCountingInstaller>();

            var first = container.Resolve<SubcontainerClass>();
            var second = container.Resolve<SubcontainerClass>();

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(SubcontainerClass.InstancesCount, Is.EqualTo(2));
            Assert.That(StaticCountingInstaller.InstallCallsCount, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_FromSubcontainerResolve_ByInstaller_WhenSubcontainerNeedsParentDependency_ResolvesParentBinding()
        {
            var dependency = new Class();

            var container = new Container();
            container.Bind<Class>().FromInstance(dependency);
            container.Bind<ClassWithParentDependency>().FromSubcontainerResolve()
                .ByInstaller(new ParentDependencyInstaller()).AsTransient();

            var resolved = container.Resolve<ClassWithParentDependency>();

            Assert.That(resolved.Dependency, Is.SameAs(dependency));
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
