using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;
using static Uniject.Tests.Performance.Memory.ContainerMemoryFixtures;

namespace Uniject.Tests.Performance.Memory
{
    public class FactoryCreateMemoryTests
    {
        [Test, Performance]
        public void Create_FromConstructor_WithCachedDependencies([Values(0, 1, 3, 10)] int dependencyCount)
        {
            using var container = new Container();
            var dependency = new Dependency();
            container.Bind<Dependency>().FromInstance(dependency);
            Func<object> create = dependencyCount switch
            {
                0 => ConstructorFactory<Service>(container),
                1 => ConstructorFactory<Service1>(container),
                3 => ConstructorFactory<Service3>(container),
                _ => ConstructorFactory<Service10>(container)
            };
            var direct = DirectConstructor(dependencyCount, dependency);
            object result = null;
            AllocationMeasurement.Run("DirectNew", () => result = direct());
            AllocationMeasurement.Run("Factory.Constructor", () => result = create());
            Assert.That(result, Is.TypeOf(ServiceType(dependencyCount)));
        }

        [Test, Performance]
        public void Create_FromConstructor_FirstCreateWithWarmReflection()
        {
            using var warmup = new Container();
            warmup.Instantiate<Service>();
            Container container = null;
            Factory<Service> factory = null;
            Service result = null;
            AllocationMeasurement.Run("Factory.FirstCreate", () => result = factory.Create(),
                setUp: () =>
                {
                    container = new Container();
                    container.BindFactory<Service, Factory<Service>>().FromConstructor().AsCached();
                    factory = container.Resolve<Factory<Service>>();
                }, cleanUp: () => container?.Dispose(), iterations: 1);
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Create_FromMethod_WithDependency()
        {
            using var container = new Container();
            container.Bind<Dependency>().FromInstance(new Dependency());
            container.BindFactory<Service1, Factory<Service1>>()
                .FromMethod(context => new Service1(context.Container.Resolve<Dependency>())).AsCached();
            var factory = container.Resolve<Factory<Service1>>();
            Service1 result = null;
            AllocationMeasurement.Run("Factory.Method", () => result = factory.Create());
            Assert.That(result.Dependency, Is.SameAs(container.Resolve<Dependency>()));
        }

        [Test, Performance]
        public void Create_FromCustomFactory()
        {
            using var container = new Container();
            container.BindFactory<Service, Factory<Service>>().FromFactory<ServiceCustomFactory>().AsCached();
            var factory = container.Resolve<Factory<Service>>();
            Service result = null;
            AllocationMeasurement.Run("Factory.Custom", () => result = factory.Create());
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Create_FromResolve([Values] bool cachedSource)
        {
            using var container = new Container();
            if (cachedSource) container.Bind<Service>().AsCached(); else container.Bind<Service>().AsTransient();
            container.BindFactory<Service, Factory<Service>>().FromResolve().AsCached();
            var factory = container.Resolve<Factory<Service>>();
            Service result = null;
            AllocationMeasurement.Run("Factory.Resolve", () => result = factory.Create(), expectZero: cachedSource);
            Assert.That(result, Is.Not.Null);
        }

        [Test, Performance]
        public void Create_WithValueParameter([Values] bool customFactory)
        {
            MeasureParameter(42, customFactory);
        }

        [Test, Performance]
        public void Create_WithReferenceParameter([Values] bool customFactory)
        {
            MeasureParameter(new Service(), customFactory);
        }

        private static void MeasureParameter<T>(T parameter, bool customFactory)
        {
            using var container = new Container();
            var binding = container.BindFactory<T, Product<T>, Factory<T, Product<T>>>();
            if (customFactory) binding.FromFactory<ParameterCustomFactory<T>>().AsCached();
            else binding.FromMethod((value, _) => new Product<T>(value)).AsCached();
            var factory = container.Resolve<Factory<T, Product<T>>>();
            Product<T> result = null;
            AllocationMeasurement.Run("DirectNew", () => result = new Product<T>(parameter));
            AllocationMeasurement.Run("Factory.Parameter", () => result = factory.Create(parameter));
            Assert.That(result.Parameter, Is.EqualTo(parameter));
        }

        private static Func<object> ConstructorFactory<T>(Container container)
        {
            container.BindFactory<T, Factory<T>>().FromConstructor().AsCached();
            var factory = container.Resolve<Factory<T>>();
            return () => factory.Create();
        }

        public sealed class ServiceCustomFactory : CustomFactory<Service>
        {
            [Preserve] public ServiceCustomFactory() { }
            public override Service Create() => new();
        }

        public sealed class Product<T>
        {
            public readonly T Parameter;
            public Product(T parameter) => Parameter = parameter;
        }

        public sealed class ParameterCustomFactory<T> : CustomFactory<T, Product<T>>
        {
            [Preserve] public ParameterCustomFactory() { }
            public override Product<T> Create(T parameter) => new(parameter);
        }
    }
}
