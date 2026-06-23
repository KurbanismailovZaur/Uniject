using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Lifecycle;
using Uniject.Tests.Fixtures;

namespace Uniject.Tests
{
    public class ContainerBuildTests
    {
        private class NonLazyClass
        {
            public static int InstancesCount { get; set; }

            public NonLazyClass() => InstancesCount++;
        }

        private class InjectableClass
        {
            public Class Dependency { get; private set; }
            public int CallsCount { get; private set; }

            [Inject]
            public void Construct(Class dependency)
            {
                Dependency = dependency;
                CallsCount++;
            }
        }

        private class EntryPointClass : IEntryPoint
        {
            public static int InstancesCount { get; set; }
            public static int RunsCount { get; set; }

            public EntryPointClass() => InstancesCount++;

            public void Run() => RunsCount++;
        }

        private static void Build(Container container)
        {
            var method = typeof(Container).GetMethod("Build", BindingFlags.Instance | BindingFlags.NonPublic);

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
        public void Build_WhenCalled_RunsNonLazyBindingsQueuedInjectionAndEntryPoints()
        {
            NonLazyClass.InstancesCount = 0;
            EntryPointClass.InstancesCount = 0;
            EntryPointClass.RunsCount = 0;

            var dependency = new Class();
            var injectable = new InjectableClass();

            var container = new Container();
            container.Bind<Class>().FromInstance(dependency);
            container.Bind<NonLazyClass>().AsTransient().NonLazy();
            container.Bind<EntryPointClass>().AsEntryPoint();
            container.AddToInjectionQueue(injectable);

            Assert.That(container.IsBuilded, Is.False);

            Build(container);

            Assert.That(container.IsBuilded, Is.True);
            Assert.That(NonLazyClass.InstancesCount, Is.EqualTo(1));
            Assert.That(injectable.Dependency, Is.SameAs(dependency));
            Assert.That(injectable.CallsCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.InstancesCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.RunsCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_WhenCalledTwice_RunsLifecycleOnlyOnce()
        {
            NonLazyClass.InstancesCount = 0;
            EntryPointClass.InstancesCount = 0;
            EntryPointClass.RunsCount = 0;

            var dependency = new Class();
            var injectable = new InjectableClass();

            var container = new Container();
            container.Bind<Class>().FromInstance(dependency);
            container.Bind<NonLazyClass>().AsTransient().NonLazy();
            container.Bind<EntryPointClass>().AsEntryPoint();
            container.AddToInjectionQueue(injectable);

            Build(container);
            Build(container);

            Assert.That(NonLazyClass.InstancesCount, Is.EqualTo(1));
            Assert.That(injectable.CallsCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.InstancesCount, Is.EqualTo(1));
            Assert.That(EntryPointClass.RunsCount, Is.EqualTo(1));
        }
    }
}
