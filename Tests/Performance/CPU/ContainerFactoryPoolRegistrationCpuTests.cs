using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerFactoryPoolRegistrationCpuTests
    {
        [Test, Performance]
        public void BindFactory_FromConstructor_AsCached()
        {
            Container container = null;

            Measure.Method(() => container.BindFactory<Service, ServiceFactory>().FromConstructor().AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ServiceFactory>();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindFactory", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindFactory_FromFactory_WithEmptyInitialize()
        {
            Container container = null;

            Measure.Method(() => container.BindFactory<Service, ServiceFactory>().FromFactory<ServiceCustomFactory>())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ServiceFactory>();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindFactory", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindFactoryWithParameter_FromMethod_AsCached()
        {
            Func<int, InjectContext, Service> factory = (value, context) => new Service();
            Container container = null;

            Measure.Method(() => container.BindFactory<int, Service, ParameterizedServiceFactory>().FromMethod(factory).AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ParameterizedServiceFactory>();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindFactory", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindFactoryWithParameter_FromFactory_WithEmptyInitialize()
        {
            Container container = null;

            Measure.Method(() => container.BindFactory<int, Service, ParameterizedServiceFactory>()
                    .FromFactory<ParameterizedServiceCustomFactory>())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ParameterizedServiceFactory>();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindFactory", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindPool_WithInitialAndMaxSize_ExpandByOne()
        {
            Container container = null;

            Measure.Method(() => container.BindPool<Service, ServicePool>()
                    .WithInitialSize(10).WithMaxSize(100).ExpandByOne().FromConstructor().AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ServicePool>().Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindPool", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindPool_WithInitialAndMaxSize_ExpandByDoubling()
        {
            Container container = null;

            Measure.Method(() => container.BindPool<Service, ServicePool>()
                    .WithInitialSize(10).WithMaxSize(100).ExpandByDoubling().FromConstructor().AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Resolve<ServicePool>().Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindPool", SampleUnit.Nanosecond))
                .Run();
        }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }

        private sealed class ServiceFactory : Factory<Service>
        {
            [Preserve]
            public ServiceFactory() { }
        }

        private sealed class ParameterizedServiceFactory : Factory<int, Service>
        {
            [Preserve]
            public ParameterizedServiceFactory() { }
        }

        private sealed class ServiceCustomFactory : CustomFactory<Service>
        {
            [Preserve]
            public ServiceCustomFactory() { }

            protected override void Initialize() { }

            public override Service Create() => new Service();
        }

        private sealed class ParameterizedServiceCustomFactory : CustomFactory<int, Service>
        {
            [Preserve]
            public ParameterizedServiceCustomFactory() { }

            protected override void Initialize() { }

            public override Service Create(int value) => new Service();
        }

        private sealed class ServicePool : Pool<Service>
        {
            [Preserve]
            public ServicePool() { }
        }
    }
}
