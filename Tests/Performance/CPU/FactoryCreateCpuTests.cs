using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class FactoryCreateCpuTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void Create_FromConstructor_AfterWarmup(int dependencyCount)
        {
            using var container = new Container();
            BindCachedDependencies(container, dependencyCount);
            var create = dependencyCount switch
            {
                0 => BindConstructorFactory<Service>(container),
                1 => BindConstructorFactory<ServiceWith1Dependency>(container),
                3 => BindConstructorFactory<ServiceWith3Dependencies>(container),
                _ => BindConstructorFactory<ServiceWith10Dependencies>(container)
            };
            create();

            Measure.Method(create)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Create_FromConstructor_FirstCreateWithWarmReflectionCache()
        {
            using var warmupContainer = new Container();
            warmupContainer.Instantiate<Service>();
            Container container = null;
            Factory<Service> factory = null;

            Measure.Method(() => factory.Create())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindFactory<Service, Factory<Service>>().FromConstructor().AsCached();
                    factory = container.Resolve<Factory<Service>>();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Create_FromMethod_WithInjectContextContainer()
        {
            using var container = new Container();
            BindCachedDependencies(container, 1);
            container.BindFactory<ServiceWith1Dependency, Factory<ServiceWith1Dependency>>()
                .FromMethod(context => new ServiceWith1Dependency(context.Container.Resolve<Dependency1>()))
                .AsCached();
            var factory = container.Resolve<Factory<ServiceWith1Dependency>>();
            factory.Create();

            Measure.Method(() => factory.Create())
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Create_FromCustomFactory()
        {
            using var container = new Container();
            container.BindFactory<Service, Factory<Service>>().FromFactory<ServiceCustomFactory>().AsCached();
            var factory = container.Resolve<Factory<Service>>();
            factory.Create();

            Measure.Method(() => factory.Create())
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Create_FromResolve(bool cachedSource)
        {
            using var container = new Container();
            if (cachedSource)
                container.Bind<Service>().AsCached();
            else
                container.Bind<Service>().AsTransient();

            container.BindFactory<Service, Factory<Service>>().FromResolve().AsCached();
            var factory = container.Resolve<Factory<Service>>();
            factory.Create();

            Measure.Method(() => factory.Create())
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Create_FromNewComponentOn()
        {
            using var container = new Container();
            var gameObject = new GameObject("Target");
            container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                .FromNewComponentOn(gameObject)
                .AsCached();
            var factory = container.Resolve<Factory<PlainComponent>>();
            PlainComponent instance = null;

            Measure.Method(() => instance = factory.Create())
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Create_FromNewComponentOnRoot(bool sceneContext)
        {
            var root = new GameObject("Context");
            Context context = sceneContext
                ? root.AddComponent<SceneContext>()
                : root.AddComponent<GameObjectContext>();
            context.Initialize();
            using var container = new Container(context.Container);
            container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                .FromNewComponentOnRoot()
                .AsCached();
            var factory = container.Resolve<Factory<PlainComponent>>();
            PlainComponent instance = null;

            Measure.Method(() => instance = factory.Create())
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Create_FromComponentInHierarchy_TargetLast(int objectCount)
        {
            var root = new GameObject("Context");
            var context = root.AddComponent<GameObjectContext>();
            context.Initialize();
            var target = root;

            for (var i = 1; i < objectCount; i++)
            {
                target = new GameObject("Child");
                target.transform.SetParent(root.transform);
            }

            target.AddComponent<PlainComponent>();
            var container = context.Container;
            container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                .FromComponentInHierarchy()
                .AsCached();
            var factory = container.Resolve<Factory<PlainComponent>>();
            factory.Create();

            Measure.Method(() => factory.Create())
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Create_FromComponentInNewPrefab(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");
            var prefabComponent = prefab.AddComponent<PlainComponent>();

            if (componentArgument)
                container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                    .FromComponentInNewPrefab(prefabComponent).AsCached();
            else
                container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                    .FromComponentInNewPrefab(prefab).AsCached();

            var factory = container.Resolve<Factory<PlainComponent>>();
            PlainComponent instance = null;

            Measure.Method(() => instance = factory.Create())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void Create_FromNewComponentOnNewPrefab(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");

            if (componentArgument)
                container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                    .FromNewComponentOnNewPrefab(prefab.transform).AsCached();
            else
                container.BindFactory<PlainComponent, Factory<PlainComponent>>()
                    .FromNewComponentOnNewPrefab(prefab).AsCached();

            var factory = container.Resolve<Factory<PlainComponent>>();
            PlainComponent instance = null;

            Measure.Method(() => instance = factory.Create())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [Test, Performance]
        public void Create_FromNewComponentOnNewGameObject_WithInjection()
        {
            using var container = new Container();
            BindCachedDependencies(container, 1);
            container.BindFactory<InjectedComponent, Factory<InjectedComponent>>()
                .FromNewComponentOnNewGameObject()
                .AsCached();
            var factory = container.Resolve<Factory<InjectedComponent>>();
            InjectedComponent instance = null;

            Measure.Method(() => instance = factory.Create())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        public sealed class PlainComponent : MonoBehaviour { }

        public sealed class InjectedComponent : MonoBehaviour
        {
            [Inject]
            private void Construct(Dependency1 dependency) { }
        }

        [Test, Performance]
        public void CreateWithParameter_FromMethod_ValueType()
        {
            using var container = new Container();
            var parameter = 42;
            Func<int, InjectContext, ParameterProduct<int>> method =
                (value, _) => new ParameterProduct<int>(value);
            container.BindFactory<int, ParameterProduct<int>, Factory<int, ParameterProduct<int>>>()
                .FromMethod(method).AsCached();
            var factory = container.Resolve<Factory<int, ParameterProduct<int>>>();

            Measure.Method(() => factory.Create(parameter))
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void CreateWithParameter_FromMethod_ReferenceType()
        {
            using var container = new Container();
            var parameter = new Parameter();
            Func<Parameter, InjectContext, ParameterProduct<Parameter>> method =
                (value, _) => new ParameterProduct<Parameter>(value);
            container.BindFactory<Parameter, ParameterProduct<Parameter>, Factory<Parameter, ParameterProduct<Parameter>>>()
                .FromMethod(method).AsCached();
            var factory = container.Resolve<Factory<Parameter, ParameterProduct<Parameter>>>();

            Measure.Method(() => factory.Create(parameter))
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void CreateWithParameter_FromFactory_ValueType()
        {
            using var container = new Container();
            var parameter = 42;
            container.BindFactory<int, ParameterProduct<int>, Factory<int, ParameterProduct<int>>>()
                .FromFactory<ParameterCustomFactory<int>>().AsCached();
            var factory = container.Resolve<Factory<int, ParameterProduct<int>>>();

            Measure.Method(() => factory.Create(parameter))
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void CreateWithParameter_FromFactory_ReferenceType()
        {
            using var container = new Container();
            var parameter = new Parameter();
            container.BindFactory<Parameter, ParameterProduct<Parameter>, Factory<Parameter, ParameterProduct<Parameter>>>()
                .FromFactory<ParameterCustomFactory<Parameter>>().AsCached();
            var factory = container.Resolve<Factory<Parameter, ParameterProduct<Parameter>>>();

            Measure.Method(() => factory.Create(parameter))
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void CreateWithParameter_FromComponentInNewPrefab(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");
            Component component = prefab.AddComponent<PlainComponent>();
            PlainComponent instance = null;
            Action create;
            if (componentArgument)
            {
                container.BindFactory<Component, PlainComponent, Factory<Component, PlainComponent>>()
                    .FromComponentInNewPrefab().AsCached();
                var factory = container.Resolve<Factory<Component, PlainComponent>>();
                create = () => instance = factory.Create(component);
            }
            else
            {
                container.BindFactory<GameObject, PlainComponent, Factory<GameObject, PlainComponent>>()
                    .FromComponentInNewPrefab().AsCached();
                var factory = container.Resolve<Factory<GameObject, PlainComponent>>();
                create = () => instance = factory.Create(prefab);
            }

            Measure.Method(create)
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void CreateWithParameter_FromNewComponentOnNewPrefab(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");
            Component component = prefab.transform;
            PlainComponent instance = null;
            Action create;
            if (componentArgument)
            {
                container.BindFactory<Component, PlainComponent, Factory<Component, PlainComponent>>()
                    .FromNewComponentOnNewPrefab().AsCached();
                var factory = container.Resolve<Factory<Component, PlainComponent>>();
                create = () => instance = factory.Create(component);
            }
            else
            {
                container.BindFactory<GameObject, PlainComponent, Factory<GameObject, PlainComponent>>()
                    .FromNewComponentOnNewPrefab().AsCached();
                var factory = container.Resolve<Factory<GameObject, PlainComponent>>();
                create = () => instance = factory.Create(prefab);
            }

            Measure.Method(create)
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Create", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        private sealed class Parameter { }

        private sealed class ParameterProduct<T>
        {
            public T Parameter { get; }

            public ParameterProduct(T parameter) => Parameter = parameter;
        }

        private sealed class ParameterCustomFactory<T> : CustomFactory<T, ParameterProduct<T>>
        {
            [Preserve]
            public ParameterCustomFactory() { }

            public override ParameterProduct<T> Create(T parameter) => new(parameter);
        }

        private static Action BindConstructorFactory<T>(Container container)
        {
            container.BindFactory<T, Factory<T>>().FromConstructor().AsCached();
            var factory = container.Resolve<Factory<T>>();
            return () => factory.Create();
        }

        private static void BindCachedDependencies(Container container, int count)
        {
            var types = new[]
            {
                typeof(Dependency1), typeof(Dependency2), typeof(Dependency3), typeof(Dependency4), typeof(Dependency5),
                typeof(Dependency6), typeof(Dependency7), typeof(Dependency8), typeof(Dependency9), typeof(Dependency10)
            };

            for (var i = 0; i < count; i++)
            {
                container.Bind(types[i]).AsCached();
                container.Resolve(types[i]);
            }
        }

        private sealed class Service
        {
            [Preserve]
            public Service() { }
        }

        private sealed class ServiceCustomFactory : CustomFactory<Service>
        {
            [Preserve]
            public ServiceCustomFactory() { }

            public override Service Create() => new Service();
        }

        private sealed class Dependency1
        {
            [Preserve]
            public Dependency1() { }
        }

        private sealed class Dependency2
        {
            [Preserve]
            public Dependency2() { }
        }

        private sealed class Dependency3
        {
            [Preserve]
            public Dependency3() { }
        }

        private sealed class Dependency4
        {
            [Preserve]
            public Dependency4() { }
        }

        private sealed class Dependency5
        {
            [Preserve]
            public Dependency5() { }
        }

        private sealed class Dependency6
        {
            [Preserve]
            public Dependency6() { }
        }

        private sealed class Dependency7
        {
            [Preserve]
            public Dependency7() { }
        }

        private sealed class Dependency8
        {
            [Preserve]
            public Dependency8() { }
        }

        private sealed class Dependency9
        {
            [Preserve]
            public Dependency9() { }
        }

        private sealed class Dependency10
        {
            [Preserve]
            public Dependency10() { }
        }

        private sealed class ServiceWith1Dependency
        {
            [Preserve]
            public ServiceWith1Dependency(Dependency1 dependency) { }
        }

        private sealed class ServiceWith3Dependencies
        {
            [Preserve]
            public ServiceWith3Dependencies(Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3) { }
        }

        private sealed class ServiceWith10Dependencies
        {
            [Preserve]
            public ServiceWith10Dependencies(
                Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3,
                Dependency4 dependency4, Dependency5 dependency5, Dependency6 dependency6,
                Dependency7 dependency7, Dependency8 dependency8, Dependency9 dependency9,
                Dependency10 dependency10) { }
        }
    }
}
