using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Contexts;
using Uniject.Installers;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerResolveCpuTests
    {
        [Test, Performance]
        public void ResolveGeneric_AsCached_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<Service>().AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_AsCached_FirstResolveWithWarmReflectionCache()
        {
            using var warmupContainer = new Container();
            warmupContainer.Bind<Service>().AsCached();
            warmupContainer.Resolve<Service>();

            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_AsTransient_WithoutDependencies()
        {
            using var container = new Container();
            container.Bind<Service>().AsTransient();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(typeof(ServiceWith1Dependency))]
        [TestCase(typeof(ServiceWith3Dependencies))]
        [TestCase(typeof(ServiceWith10Dependencies))]
        [Performance]
        public void ResolveType_AsTransient_WithPreResolvedCachedDependencies(Type serviceType)
        {
            using var container = new Container();

            foreach (var parameter in serviceType.GetConstructors()[0].GetParameters())
            {
                container.Bind(parameter.ParameterType).AsCached();
                container.Resolve(parameter.ParameterType);
            }

            container.Bind(serviceType).AsTransient();
            container.Resolve(serviceType);

            Measure.Method(() => container.Resolve(serviceType))
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Performance]
        public void ResolveType_AsTransient_ChainIncludingRoot(int objectCount)
        {
            using var container = new Container();
            var types = new[]
            {
                typeof(Service), typeof(Chain2), typeof(Chain3), typeof(Chain4), typeof(Chain5),
                typeof(Chain6), typeof(Chain7), typeof(Chain8), typeof(Chain9), typeof(Chain10)
            };

            for (var i = 0; i < objectCount; i++)
                container.Bind(types[i]).AsTransient();

            var rootType = types[objectCount - 1];
            container.Resolve(rootType);

            Measure.Method(() => container.Resolve(rootType))
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void ResolveGeneric_AsCached_FromAncestor(int depth)
        {
            var containers = new Container[depth + 1];
            containers[0] = new Container();
            containers[0].Bind<Service>().AsCached();
            containers[0].Resolve<Service>();

            for (var i = 1; i < containers.Length; i++)
                containers[i] = new Container(containers[i - 1]);

            var leaf = containers[depth];
            leaf.Resolve<Service>();

            Measure.Method(() => leaf.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            for (var i = containers.Length - 1; i >= 0; i--)
                containers[i].Dispose();
        }

        [Test, Performance]
        public void ResolveType_AsCached_FirstResolve()
        {
            Container container = null;
            var serviceType = typeof(Service);

            Measure.Method(() => container.Resolve(serviceType))
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveType_AsCached_AfterWarmup()
        {
            using var container = new Container();
            var serviceType = typeof(Service);
            container.Bind<Service>().AsCached();
            container.Resolve(serviceType);

            Measure.Method(() => container.Resolve(serviceType))
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_AsCached_FirstResolveWith1Dependency([Values(false, true)] bool preResolveDependencies)
        {
            MeasureFirstCachedResolve<ServiceWith1Dependency>(preResolveDependencies);
        }

        [Test, Performance]
        public void ResolveGeneric_AsCached_FirstResolveWith3Dependencies([Values(false, true)] bool preResolveDependencies)
        {
            MeasureFirstCachedResolve<ServiceWith3Dependencies>(preResolveDependencies);
        }

        [Test, Performance]
        public void ResolveGeneric_AsCached_FirstResolveWith10Dependencies([Values(false, true)] bool preResolveDependencies)
        {
            MeasureFirstCachedResolve<ServiceWith10Dependencies>(preResolveDependencies);
        }

        [Test, Performance]
        public void ResolveGeneric_AsTransient_ThroughInterface()
        {
            using var container = new Container();
            container.Bind<IService>().To<Service>().AsTransient();
            container.Resolve<IService>();

            Measure.Method(() => container.Resolve<IService>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void ResolveGeneric_AsCached_WithUserBindings(int bindingCount)
        {
            using var container = new Container();
            var types = new[]
            {
                typeof(Dependency1), typeof(Dependency2), typeof(Dependency3), typeof(Dependency4), typeof(Dependency5),
                typeof(Dependency6), typeof(Dependency7), typeof(Dependency8), typeof(Dependency9), typeof(Dependency10)
            };

            for (var i = 0; i < bindingCount - 1; i++)
                container.Bind(typeof(ExtraBinding<,,>).MakeGenericType(types[i / 100], types[i / 10 % 10], types[i % 10])).AsCached();

            container.Bind<Service>().AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromInstance_FirstResolve()
        {
            var instance = new Service();
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().FromInstance(instance);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromInstance_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<Service>().FromInstance(new Service());
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromMethod_AsTransient_WithoutDependencies()
        {
            using var container = new Container();
            container.Bind<Service>().FromMethod(_ => new Service()).AsTransient();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromMethod_AsTransient_WithDependency()
        {
            using var container = new Container();
            container.Bind<Dependency1>().AsCached();
            container.Bind<ServiceWith1Dependency>()
                .FromMethod(context => new ServiceWith1Dependency(context.Container.Resolve<Dependency1>()))
                .AsTransient();
            container.Resolve<ServiceWith1Dependency>();

            Measure.Method(() => container.Resolve<ServiceWith1Dependency>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromResolve_AsTransient([Values(false, true)] bool cachedSource)
        {
            using var container = new Container();
            if (cachedSource)
                container.Bind<Service>().AsCached();
            else
                container.Bind<Service>().AsTransient();

            container.Bind<IService>().To<Service>().FromResolve().AsTransient();
            container.Resolve<IService>();

            Measure.Method(() => container.Resolve<IService>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromResolveGetter_AsTransient()
        {
            using var container = new Container();
            container.Bind<ServiceSource>().AsCached();
            container.Resolve<ServiceSource>();
            container.Bind<Service>().FromResolveGetter<ServiceSource>(source => source.Service).AsTransient();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_DisposeWithContainer_FirstResolve()
        {
            Container container = null;

            Measure.Method(() => container.Resolve<DisposableService>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<DisposableService>().AsCached().DisposeWithContainer();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_DisposeWithContainer_AfterWarmup()
        {
            using var container = new Container();
            container.Bind<DisposableService>().AsCached().DisposeWithContainer();
            container.Resolve<DisposableService>();

            Measure.Method(() => container.Resolve<DisposableService>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewComponentOn_AsTransient(bool withInjection)
        {
            using var container = new Container();
            var gameObject = new GameObject("Target");
            Component instance = null;
            Action resolve;

            if (withInjection)
            {
                container.Bind<Service>().AsCached();
                container.Resolve<Service>();
                container.Bind<UnityCpuInjectedComponent>().FromNewComponentOn(gameObject).AsTransient();
                resolve = () => instance = container.Resolve<UnityCpuInjectedComponent>();
            }
            else
            {
                container.Bind<UnityCpuComponent>().FromNewComponentOn(gameObject).AsTransient();
                resolve = () => instance = container.Resolve<UnityCpuComponent>();
            }

            Measure.Method(resolve)
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewComponentOnRoot_AsTransient(bool sceneContext)
        {
            var root = new GameObject("Context");
            Context context = sceneContext
                ? root.AddComponent<SceneContext>()
                : root.AddComponent<GameObjectContext>();
            context.Initialize();
            using var container = new Container(context.Container);
            container.Bind<UnityCpuComponent>().FromNewComponentOnRoot().AsTransient();
            UnityCpuComponent instance = null;

            Measure.Method(() => instance = container.Resolve<UnityCpuComponent>())
                .CleanUp(() => Object.DestroyImmediate(instance))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        [TestCase(false, 1)]
        [TestCase(false, 10)]
        [TestCase(false, 100)]
        [TestCase(false, 1000)]
        [TestCase(true, 1)]
        [TestCase(true, 10)]
        [TestCase(true, 100)]
        [TestCase(true, 1000)]
        [Performance]
        public void ResolveGeneric_FromComponentInHierarchy_AsTransient_TargetLast(bool sceneContext, int objectCount)
        {
            var scene = SceneManager.CreateScene($"Hierarchy-{Guid.NewGuid()}");
            var root = new GameObject("Context");
            SceneManager.MoveGameObjectToScene(root, scene);
            Context context = sceneContext
                ? root.AddComponent<SceneContext>()
                : root.AddComponent<GameObjectContext>();
            var target = root;

            for (var i = 1; i < objectCount; i++)
            {
                target = new GameObject("Child");
                target.transform.SetParent(root.transform);
            }

            target.AddComponent<UnityCpuComponent>();
            context.Initialize();
            var container = context.Container;
            container.Bind<UnityCpuComponent>().FromComponentInHierarchy().AsTransient();
            container.Resolve<UnityCpuComponent>();

            Measure.Method(() => container.Resolve<UnityCpuComponent>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
            SceneManager.UnloadSceneAsync(scene);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromComponentInNewPrefab_AsTransient(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");
            var prefabComponent = prefab.AddComponent<UnityCpuComponent>();

            if (componentArgument)
                container.Bind<UnityCpuComponent>().FromComponentInNewPrefab(prefabComponent).AsTransient();
            else
                container.Bind<UnityCpuComponent>().FromComponentInNewPrefab(prefab).AsTransient();

            UnityCpuComponent instance = null;

            Measure.Method(() => instance = container.Resolve<UnityCpuComponent>())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewComponentOnNewPrefab_AsTransient(bool componentArgument)
        {
            using var container = new Container();
            var prefab = new GameObject("Prefab");

            if (componentArgument)
                container.Bind<UnityCpuComponent>().FromNewComponentOnNewPrefab(prefab.transform).AsTransient();
            else
                container.Bind<UnityCpuComponent>().FromNewComponentOnNewPrefab(prefab).AsTransient();

            UnityCpuComponent instance = null;

            Measure.Method(() => instance = container.Resolve<UnityCpuComponent>())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewComponentOnNewGameObject_AsTransient(bool withInjection)
        {
            using var container = new Container();
            Component instance = null;
            Action resolve;

            if (withInjection)
            {
                container.Bind<Service>().AsCached();
                container.Resolve<Service>();
                container.Bind<UnityCpuInjectedComponent>().FromNewComponentOnNewGameObject().AsTransient();
                resolve = () => instance = container.Resolve<UnityCpuInjectedComponent>();
            }
            else
            {
                container.Bind<UnityCpuComponent>().FromNewComponentOnNewGameObject().AsTransient();
                resolve = () => instance = container.Resolve<UnityCpuComponent>();
            }

            Measure.Method(resolve)
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_WithGameObjectNameUnderTransform_AsTransient()
        {
            using var container = new Container();
            var parent = new GameObject("Parent");
            container.Bind<UnityCpuComponent>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("NamedObject")
                .UnderTransform(parent.transform)
                .AsTransient();
            UnityCpuComponent instance = null;

            Measure.Method(() => instance = container.Resolve<UnityCpuComponent>())
                .CleanUp(() => Object.DestroyImmediate(instance.gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
        }

        public sealed class UnityCpuComponent : MonoBehaviour { }

        public sealed class UnityCpuInjectedComponent : MonoBehaviour
        {
            [Inject]
            private void Construct(Service dependency) { }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByInstance_FirstResolve(bool cachedService)
        {
            Container container = null;
            Container subcontainer = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    subcontainer = new Container();
                    InstallService(subcontainer, cachedService);
                    subcontainer.Build();
                    container = new Container();
                    container.Bind<Service>().FromSubcontainerResolve().ByInstance(subcontainer).AsCached();
                })
                .CleanUp(() =>
                {
                    subcontainer.Dispose();
                    container.Dispose();
                })
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByInstance_AfterWarmup(bool cachedService)
        {
            using var container = new Container();
            using var subcontainer = new Container();
            InstallService(subcontainer, cachedService);
            subcontainer.Build();
            container.Bind<Service>().FromSubcontainerResolve().ByInstance(subcontainer).AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByMethod_AsCached_FirstResolve(bool cachedService)
        {
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().FromSubcontainerResolve()
                        .ByMethod(child => InstallService(child, cachedService)).AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByMethod_AsCached_AfterWarmup(bool cachedService)
        {
            using var container = new Container();
            container.Bind<Service>().FromSubcontainerResolve()
                .ByMethod(child => InstallService(child, cachedService)).AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromSubcontainerByMethod_AsTransient()
        {
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.Bind<Service>().FromSubcontainerResolve()
                        .ByMethod(child => child.Bind<Service>().AsCached()).AsTransient();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByInstaller_AsCached_FirstResolve(bool installerInstance)
        {
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container();
                    var binding = container.Bind<Service>().FromSubcontainerResolve();
                    if (installerInstance)
                        binding.ByInstaller(new SubcontainerCpuInstaller()).AsCached();
                    else
                        binding.ByInstaller<SubcontainerCpuInstaller>().AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromSubcontainerByInstaller_AsCached_AfterWarmup(bool installerInstance)
        {
            using var container = new Container();
            var binding = container.Bind<Service>().FromSubcontainerResolve();
            if (installerInstance)
                binding.ByInstaller(new SubcontainerCpuInstaller()).AsCached();
            else
                binding.ByInstaller<SubcontainerCpuInstaller>().AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_FromNewContextByMethodOnNewGameObject_FirstResolve()
        {
            var parent = new GameObject("ContextParent");
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container(parentTransformForGameObjects: parent.transform);
                    container.Bind<Service>().FromSubcontainerResolve()
                        .ByNewContextFromMethodOnNewGameObject(child => child.Bind<Service>().AsCached())
                        .AsCached();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
        }

        [Test, Performance]
        public void ResolveGeneric_FromNewContextByMethodOnNewGameObject_AfterWarmup()
        {
            var parent = new GameObject("ContextParent");
            using var container = new Container(parentTransformForGameObjects: parent.transform);
            container.Bind<Service>().FromSubcontainerResolve()
                .ByNewContextFromMethodOnNewGameObject(child => child.Bind<Service>().AsCached())
                .AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewContextByInstallerOnNewGameObject_FirstResolve(bool installerInstance)
        {
            var parent = new GameObject("ContextParent");
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container(parentTransformForGameObjects: parent.transform);
                    var binding = container.Bind<Service>().FromSubcontainerResolve();
                    if (installerInstance)
                        binding.ByNewContextFromInstallerOnNewGameObject(new SubcontainerCpuInstaller()).AsCached();
                    else
                        binding.ByNewContextFromInstallerOnNewGameObject<SubcontainerCpuInstaller>().AsCached();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
        }

        [Test, Performance]
        public void ResolveGeneric_FromNewContextByMethodOnNewPrefab_FirstResolve()
        {
            var prefab = new GameObject("ContextPrefab");
            var parent = new GameObject("ContextParent");
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container(parentTransformForGameObjects: parent.transform);
                    container.Bind<Service>().FromSubcontainerResolve()
                        .ByNewContextFromMethodOnNewPrefab(prefab, child => child.Bind<Service>().AsCached())
                        .AsCached();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        [Test, Performance]
        public void ResolveGeneric_FromNewContextByMethodOnNewPrefab_AfterWarmup()
        {
            var prefab = new GameObject("ContextPrefab");
            var parent = new GameObject("ContextParent");
            using var container = new Container(parentTransformForGameObjects: parent.transform);
            container.Bind<Service>().FromSubcontainerResolve()
                .ByNewContextFromMethodOnNewPrefab(prefab, child => child.Bind<Service>().AsCached())
                .AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void ResolveGeneric_FromNewContextByInstallerOnNewPrefab_FirstResolve(bool installerInstance)
        {
            var prefab = new GameObject("ContextPrefab");
            var parent = new GameObject("ContextParent");
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container(parentTransformForGameObjects: parent.transform);
                    var binding = container.Bind<Service>().FromSubcontainerResolve();
                    if (installerInstance)
                        binding.ByNewContextFromInstallerOnNewPrefab(prefab, new SubcontainerCpuInstaller()).AsCached();
                    else
                        binding.ByNewContextFromInstallerOnNewPrefab<SubcontainerCpuInstaller>(prefab).AsCached();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        [Test, Performance]
        public void ResolveGeneric_FromSubcontainerByContextOnNewPrefab_FirstResolve()
        {
            var prefab = new GameObject("ContextPrefab");
            prefab.AddComponent<GameObjectContext>();
            prefab.AddComponent<SubcontainerCpuMonoInstaller>();
            var parent = new GameObject("ContextParent");
            Container container = null;

            Measure.Method(() => container.Resolve<Service>())
                .SetUp(() =>
                {
                    container = new Container(parentTransformForGameObjects: parent.transform);
                    container.Bind<Service>().FromSubcontainerResolve().ByContextOnNewPrefab(prefab).AsCached();
                })
                .CleanUp(() =>
                {
                    Object.DestroyImmediate(parent.transform.GetChild(0).gameObject);
                    container.Dispose();
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        [Test, Performance]
        public void ResolveGeneric_FromSubcontainerByContextOnNewPrefab_AfterWarmup()
        {
            var prefab = new GameObject("ContextPrefab");
            prefab.AddComponent<GameObjectContext>();
            prefab.AddComponent<SubcontainerCpuMonoInstaller>();
            var parent = new GameObject("ContextParent");
            using var container = new Container(parentTransformForGameObjects: parent.transform);
            container.Bind<Service>().FromSubcontainerResolve().ByContextOnNewPrefab(prefab).AsCached();
            container.Resolve<Service>();

            Measure.Method(() => container.Resolve<Service>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(prefab);
        }

        private static void InstallService(Container container, bool cached)
        {
            var binding = container.Bind<Service>();
            if (cached)
                binding.AsCached();
            else
                binding.AsTransient();
        }

        private sealed class SubcontainerCpuInstaller : IInstaller
        {
            [Preserve]
            public SubcontainerCpuInstaller() { }

            public void Install(Container container) => container.Bind<Service>().AsCached();
        }

        [Test, Performance]
        public void ResolveGeneric_BindFactory_AsCached_FirstResolve()
        {
            Container container = null;

            Measure.Method(() => container.Resolve<FactoryCpu>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindFactory<Service, FactoryCpu>().AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void ResolveGeneric_BindFactory_AsCached_AfterWarmup()
        {
            using var container = new Container();
            container.BindFactory<Service, FactoryCpu>().AsCached();
            container.Resolve<FactoryCpu>();

            Measure.Method(() => container.Resolve<FactoryCpu>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void ResolveGeneric_BindPool_AsCached_FirstResolve(int initialSize)
        {
            Container container = null;

            Measure.Method(() => container.Resolve<PoolCpu>())
                .SetUp(() =>
                {
                    container = new Container();
                    container.BindPool<Service, PoolCpu>().WithInitialSize(initialSize).AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void ResolveGeneric_BindPool_AsCached_AfterWarmup(int initialSize)
        {
            using var container = new Container();
            container.BindPool<Service, PoolCpu>().WithInitialSize(initialSize).AsCached();
            container.Resolve<PoolCpu>();

            Measure.Method(() => container.Resolve<PoolCpu>())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        private sealed class FactoryCpu : Factory<Service>
        {
            [Preserve]
            public FactoryCpu() { }
        }

        private sealed class PoolCpu : Pool<Service>
        {
            [Preserve]
            public PoolCpu() { }
        }

        public sealed class SubcontainerCpuMonoInstaller : MonoInstaller
        {
            public override void Install(Container container) => container.Bind<Service>().AsCached();
        }

        private static void MeasureFirstCachedResolve<T>(bool preResolveDependencies)
        {
            var parameters = typeof(T).GetConstructors()[0].GetParameters();
            Container container = null;

            Measure.Method(() => container.Resolve<T>())
                .SetUp(() =>
                {
                    container = new Container();
                    foreach (var parameter in parameters)
                    {
                        container.Bind(parameter.ParameterType).AsCached();
                        if (preResolveDependencies)
                            container.Resolve(parameter.ParameterType);
                    }

                    container.Bind<T>().AsCached();
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Resolve", SampleUnit.Nanosecond))
                .Run();
        }

        private interface IService { }

        private sealed class ExtraBinding<T1, T2, T3> { }

        private sealed class ServiceSource
        {
            public Service Service { get; } = new();

            [Preserve]
            public ServiceSource() { }
        }

        private sealed class DisposableService : IDisposable
        {
            [Preserve]
            public DisposableService() { }

            public void Dispose() { }
        }

        private sealed class Service : IService
        {
            [Preserve]
            public Service() { }
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
            public ServiceWith1Dependency(Dependency1 dependency1) { }
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

        private sealed class Chain2
        {
            [Preserve]
            public Chain2(Service dependency) { }
        }

        private sealed class Chain3
        {
            [Preserve]
            public Chain3(Chain2 dependency) { }
        }

        private sealed class Chain4
        {
            [Preserve]
            public Chain4(Chain3 dependency) { }
        }

        private sealed class Chain5
        {
            [Preserve]
            public Chain5(Chain4 dependency) { }
        }

        private sealed class Chain6
        {
            [Preserve]
            public Chain6(Chain5 dependency) { }
        }

        private sealed class Chain7
        {
            [Preserve]
            public Chain7(Chain6 dependency) { }
        }

        private sealed class Chain8
        {
            [Preserve]
            public Chain8(Chain7 dependency) { }
        }

        private sealed class Chain9
        {
            [Preserve]
            public Chain9(Chain8 dependency) { }
        }

        private sealed class Chain10
        {
            [Preserve]
            public Chain10(Chain9 dependency) { }
        }
    }
}
