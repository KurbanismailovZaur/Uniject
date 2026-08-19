using NUnit.Framework;
using Uniject.Contexts;
using Uniject.Installers;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class TickableManagerCpuTests
    {
        [Test, Performance]
        public void RegisterTickable([Values(0, 10, 100, 1000)] int registeredCount)
        {
            var tickables = new TickableObject[registeredCount];
            for (var i = 0; i < tickables.Length; i++)
                tickables[i] = new TickableObject();
            var target = new TickableObject();
            GameObject root = null;
            TickableManager manager = null;

            Measure.Method(() => manager.RegisterTickable(target))
                .SetUp(() =>
                {
                    root = new GameObject("TickableManager");
                    manager = root.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("RegisterTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void RegisterLateTickable([Values(0, 10, 100, 1000)] int registeredCount)
        {
            var tickables = new LateTickableObject[registeredCount];
            for (var i = 0; i < tickables.Length; i++)
                tickables[i] = new LateTickableObject();
            var target = new LateTickableObject();
            GameObject root = null;
            TickableManager manager = null;

            Measure.Method(() => manager.RegisterLateTickable(target))
                .SetUp(() =>
                {
                    root = new GameObject("TickableManager");
                    manager = root.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterLateTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("RegisterLateTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void RegisterFixedTickable([Values(0, 10, 100, 1000)] int registeredCount)
        {
            var tickables = new FixedTickableObject[registeredCount];
            for (var i = 0; i < tickables.Length; i++)
                tickables[i] = new FixedTickableObject();
            var target = new FixedTickableObject();
            GameObject root = null;
            TickableManager manager = null;

            Measure.Method(() => manager.RegisterFixedTickable(target))
                .SetUp(() =>
                {
                    root = new GameObject("TickableManager");
                    manager = root.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterFixedTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("RegisterFixedTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Register_Object([Values("None", "Tick", "Late", "Fixed", "All")] string interfaces)
        {
            var target = CreateObject(interfaces);
            GameObject root = null;
            TickableManager manager = null;

            Measure.Method(() => manager.Register(target))
                .SetUp(() =>
                {
                    root = new GameObject("TickableManager");
                    manager = root.AddComponent<TickableManager>();
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Register", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void UnregisterTickable_ByPosition(
            [Values(1, 10, 100, 1000)] int count,
            [Values("First", "Middle", "Last")] string position)
        {
            GameObject gameObject = null;
            TickableManager manager = null;
            var tickables = new TickableObject[count];
            for (var i = 0; i < count; i++)
                tickables[i] = new TickableObject();

            var index = position switch
            {
                "First" => 0,
                "Middle" => count / 2,
                _ => count - 1
            };
            var target = tickables[index];

            Measure.Method(() => manager.UnregisterTickable(target))
                .SetUp(() =>
                {
                    gameObject = new GameObject("TickableManager");
                    manager = gameObject.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("UnregisterTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void UnregisterLateTickable_ByPosition(
            [Values(1, 10, 100, 1000)] int count,
            [Values("First", "Middle", "Last")] string position)
        {
            GameObject gameObject = null;
            TickableManager manager = null;
            var tickables = new LateTickableObject[count];
            for (var i = 0; i < count; i++)
                tickables[i] = new LateTickableObject();

            var index = position switch
            {
                "First" => 0,
                "Middle" => count / 2,
                _ => count - 1
            };
            var target = tickables[index];

            Measure.Method(() => manager.UnregisterLateTickable(target))
                .SetUp(() =>
                {
                    gameObject = new GameObject("TickableManager");
                    manager = gameObject.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterLateTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("UnregisterLateTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void UnregisterFixedTickable_ByPosition(
            [Values(1, 10, 100, 1000)] int count,
            [Values("First", "Middle", "Last")] string position)
        {
            GameObject gameObject = null;
            TickableManager manager = null;
            var tickables = new FixedTickableObject[count];
            for (var i = 0; i < count; i++)
                tickables[i] = new FixedTickableObject();

            var index = position switch
            {
                "First" => 0,
                "Middle" => count / 2,
                _ => count - 1
            };
            var target = tickables[index];

            Measure.Method(() => manager.UnregisterFixedTickable(target))
                .SetUp(() =>
                {
                    gameObject = new GameObject("TickableManager");
                    manager = gameObject.AddComponent<TickableManager>();
                    foreach (var tickable in tickables)
                        manager.RegisterFixedTickable(tickable);
                })
                .CleanUp(() => Object.DestroyImmediate(gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("UnregisterFixedTickable", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Unregister_Object([Values("Tick", "Late", "Fixed", "All")] string interfaces)
        {
            var target = CreateObject(interfaces);
            GameObject root = null;
            TickableManager manager = null;

            Measure.Method(() => manager.Unregister(target))
                .SetUp(() =>
                {
                    root = new GameObject("TickableManager");
                    manager = root.AddComponent<TickableManager>();
                    manager.Register(target);
                })
                .CleanUp(() => Object.DestroyImmediate(root))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Unregister", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(false)]
        [TestCase(true)]
        [Performance]
        public void TickableManagerInstaller_Install(bool sceneContext)
        {
            GameObject root = null;
            Context context = null;
            TickableManagerInstaller installer = null;

            Measure.Method(() => installer.Install(context.Container))
                .SetUp(() =>
                {
                    root = new GameObject("Context");
                    context = sceneContext
                        ? root.AddComponent<SceneContext>()
                        : root.AddComponent<GameObjectContext>();
                    context.enabled = false;
                    context.Initialize();
                    installer = root.AddComponent<TickableManagerInstaller>();
                })
                .CleanUp(() =>
                {
                    context.Build();
                    Object.DestroyImmediate(root);
                })
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void SceneLoaderInstaller_Install()
        {
            var root = new GameObject("SceneLoaderInstaller");
            var installer = root.AddComponent<SceneLoaderInstaller>();
            Container container = null;

            Measure.Method(() => installer.Install(container))
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("Install", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        private static object CreateObject(string interfaces) => interfaces switch
        {
            "Tick" => new TickableObject(),
            "Late" => new LateTickableObject(),
            "Fixed" => new FixedTickableObject(),
            "All" => new AllTickableObject(),
            _ => new object()
        };

        private sealed class TickableObject : ITickable
        {
            public void Tick() { }
        }

        private sealed class LateTickableObject : ILateTickable
        {
            public void LateTick() { }
        }

        private sealed class FixedTickableObject : IFixedTickable
        {
            public void FixedTick() { }
        }

        private sealed class AllTickableObject : ITickable, ILateTickable, IFixedTickable
        {
            public void Tick() { }

            public void LateTick() { }

            public void FixedTick() { }
        }
    }
}
