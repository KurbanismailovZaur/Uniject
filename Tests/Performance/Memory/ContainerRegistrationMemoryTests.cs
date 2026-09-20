using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class ContainerRegistrationMemoryTests
    {
        [Test, Performance]
        public void Constructor([Values] bool withParent)
        {
            using var parent = new Container();
            Container container = null;
            AllocationMeasurement.Run("Container.Constructor", () => container = new Container(withParent ? parent : null),
                cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void SetParentContainer_PreparedParent()
        {
            using var parent = new Container();
            using var container = new Container();
            AllocationMeasurement.Run("Container.SetParent", () => container.SetParentContainer(parent),
                setUp: () => container.SetParentContainer(null), expectZero: true);
        }

        [Test, Performance]
        public void Bind_TypeBatch([Values(0, 10, 100, 1000)] int bindingCount, [Values] bool cached)
        {
            var types = CreateTypes(typeof(Contract<,,>), bindingCount);
            Container container = null;
            AllocationMeasurement.Run("Bind.Batch", () =>
            {
                foreach (var type in types)
                {
                    var binding = container.Bind(type);
                    if (cached) binding.AsCached(); else binding.AsTransient();
                }
            }, setUp: () => container = new Container(), cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Bind_WithExistingUserBindings([Values(0, 10, 100, 1000)] int bindingCount, [Values] bool generic)
        {
            var types = CreateTypes(typeof(Contract<,,>), bindingCount);
            Container container = null;
            Action bind = generic ? () => container.Bind<Service>().AsCached() : () => container.Bind(typeof(Service)).AsCached();
            AllocationMeasurement.Run("Bind.Add", bind, setUp: () =>
            {
                container = new Container();
                foreach (var type in types) container.Bind(type).AsCached();
            }, cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Bind_Configuration([Values] BindingKind kind)
        {
            Container container = null;
            var instance = new Service();
            var disposable = new DisposableService();
            Func<InjectContext, Service> factory = _ => new Service();
            Action bind = kind switch
            {
                BindingKind.Interface => () => container.Bind<IService>().To<Service>().FromConstructor().AsCached(),
                BindingKind.Method => () => container.Bind<Service>().FromMethod(factory).AsTransient(),
                BindingKind.Instance => () => container.Bind<Service>().FromInstance(instance).AsCached(),
                BindingKind.NonLazyInstance => () => container.BindInstance(instance),
                BindingKind.DisposableInstance => () => container.Bind<DisposableService>().FromInstance(disposable).AsCached().DisposeWithContainer(),
                BindingKind.Factory => () => container.BindFactory<Service, Factory<Service>>().FromConstructor().AsCached(),
                BindingKind.ParameterFactory => () => container.BindFactory<int, FactoryCreateMemoryTests.Product<int>, Factory<int, FactoryCreateMemoryTests.Product<int>>>()
                    .FromMethod((value, _) => new FactoryCreateMemoryTests.Product<int>(value)).AsCached(),
                BindingKind.Pool => () => container.BindPool<Service, Pool<Service>>().FromConstructor().AsCached(),
                _ => () => container.Bind<Service>().FromSubcontainerResolve().ByInstaller<ServiceInstaller>().AsCached()
            };
            AllocationMeasurement.Run("Bind.Configuration", bind, setUp: () => container = new Container(),
                cleanUp: () => container?.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void BindInstances_PreparedArray([Values(1, 10, 100)] int instanceCount)
        {
            var types = CreateTypes(typeof(Contract<,,>), instanceCount);
            var instances = new object[instanceCount];
            for (var i = 0; i < instances.Length; i++) instances[i] = Activator.CreateInstance(types[i]);
            Container container = null;
            AllocationMeasurement.Run("BindInstances.Batch", () => container.BindInstances(instances),
                setUp: () => container = new Container(), cleanUp: () => container?.Dispose(), iterations: 1);
        }

        public enum BindingKind
        {
            Interface, Method, Instance, NonLazyInstance, DisposableInstance, Factory, ParameterFactory, Pool, Subcontainer
        }
    }
}
