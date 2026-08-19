using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerRegistrationCpuTests
    {
        [Test, Performance]
        public void Constructor_WithoutParent()
        {
            Container container = null;

            Measure.Method(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Constructor", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Constructor_WithParent()
        {
            using var parent = new Container();
            Container container = null;

            Measure.Method(() => container = new Container(parent))
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Constructor", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void SetParentContainer_WithPreparedParent()
        {
            using var parent = new Container();
            using var container = new Container();

            Measure.Method(() => container.SetParentContainer(parent))
                .SetUp(() => container.SetParentContainer(null))
                .SampleGroup(new SampleGroup("SetParentContainer", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void BindGeneric_WithExistingUserBindings(int bindingCount)
        {
            var contractTypes = CreateContractTypes(bindingCount);
            Container container = null;

            Measure.Method(() => container.Bind<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var contractType in contractTypes)
                        container.Bind(contractType);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void BindType_WithExistingUserBindings(int bindingCount)
        {
            var contractTypes = CreateContractTypes(bindingCount);
            var serviceType = typeof(Service);
            Container container = null;

            Measure.Method(() => container.Bind(serviceType))
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var contractType in contractTypes)
                        container.Bind(contractType);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindGeneric_ToGeneric_FromConstructor_AsCached()
        {
            Container container = null;

            Measure.Method(() => container.Bind<IService>().To<Service>().FromConstructor().AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindGeneric_ToType_FromConstructor_AsCached()
        {
            var serviceType = typeof(Service);
            Container container = null;

            Measure.Method(() => container.Bind<IService>().To(serviceType).FromConstructor().AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindGeneric_FromMethod_AsTransient()
        {
            Func<InjectContext, Service> factory = _ => new Service();
            Container container = null;

            Measure.Method(() => container.Bind<Service>().FromMethod(factory).AsTransient())
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindGeneric_FromInstance_AsCached()
        {
            var instance = new Service();
            Container container = null;

            Measure.Method(() => container.Bind<Service>().FromInstance(instance).AsCached())
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Bind", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void BindInstance_WithPreparedInstance()
        {
            var instance = new Service();
            Container container = null;

            Measure.Method(() => container.BindInstance<Service>(instance))
                .SetUp(() => container = new Container())
                .CleanUp(() =>
                {
                    container.Build();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("BindInstance", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void BindInstances_WithPreparedDistinctInstances(int instanceCount)
        {
            var contractTypes = CreateContractTypes(instanceCount);
            var instances = new object[instanceCount];
            for (var i = 0; i < instances.Length; i++)
                instances[i] = Activator.CreateInstance(contractTypes[i]);

            Container container = null;

            Measure.Method(() => container.BindInstances(instances))
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("BindInstances", SampleUnit.Nanosecond))
                .Run();
        }

        private static Type[] CreateContractTypes(int count)
        {
            var arguments = new[]
            {
                typeof(object), typeof(string), typeof(Type), typeof(Exception), typeof(Attribute),
                typeof(Delegate), typeof(Container), typeof(IObjectBuilder), typeof(IService), typeof(Service)
            };
            var types = new Type[count];
            for (var i = 0; i < types.Length; i++)
                types[i] = typeof(Contract<,,>).MakeGenericType(arguments[i / 100], arguments[i / 10 % 10], arguments[i % 10]);

            return types;
        }

        private interface IService { }

        private sealed class Service : IService
        {
            [Preserve]
            public Service() { }
        }

        private sealed class Contract<T1, T2, T3>
        {
            [Preserve]
            public Contract() { }
        }
    }
}
