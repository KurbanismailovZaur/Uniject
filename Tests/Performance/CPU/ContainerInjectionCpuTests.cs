using System.Collections.Generic;
using NUnit.Framework;
using Uniject.Attributes;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.CPU
{
    public class ContainerInjectionCpuTests
    {
        [Test, Performance]
        public void Inject_WithoutInjectMethod_AfterWarmup()
        {
            using var container = new Container();
            var target = new PlainTarget();
            container.Inject(target);

            Measure.Method(() => container.Inject(target))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        [Performance]
        public void Inject_WithPreResolvedCachedDependencies(int dependencyCount)
        {
            using var container = new Container();
            var types = new[]
            {
                typeof(Dependency1), typeof(Dependency2), typeof(Dependency3), typeof(Dependency4), typeof(Dependency5),
                typeof(Dependency6), typeof(Dependency7), typeof(Dependency8), typeof(Dependency9), typeof(Dependency10)
            };

            for (var i = 0; i < dependencyCount; i++)
            {
                container.Bind(types[i]).AsCached();
                container.Resolve(types[i]);
            }

            object target = dependencyCount switch
            {
                1 => new TargetWith1Dependency(),
                3 => new TargetWith3Dependencies(),
                _ => new TargetWith10Dependencies()
            };
            container.Inject(target);

            Measure.Method(() => container.Inject(target))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Performance]
        public void Inject_WithTransientDependencyChain(int dependencyCount)
        {
            using var container = new Container();
            var types = new[]
            {
                typeof(Dependency1), typeof(Chain2), typeof(Chain3), typeof(Chain4), typeof(Chain5),
                typeof(Chain6), typeof(Chain7), typeof(Chain8), typeof(Chain9), typeof(Chain10)
            };

            for (var i = 0; i < dependencyCount; i++)
                container.Bind(types[i]).AsTransient();

            object target = dependencyCount switch
            {
                1 => new TargetWith1Dependency(),
                5 => new ChainTarget<Chain5>(),
                _ => new ChainTarget<Chain10>()
            };
            container.Inject(target);

            Measure.Method(() => container.Inject(target))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void Inject_InheritedMethod_AfterWarmup(int depth)
        {
            using var container = new Container();
            container.Bind<Dependency1>().AsCached();
            container.Resolve<Dependency1>();
            object target = depth switch
            {
                1 => new Inherited1(),
                4 => new Inherited4(),
                _ => new Inherited16()
            };
            container.Inject(target);

            Measure.Method(() => container.Inject(target))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void InjectEnumerable_WithPreparedArray(int objectCount)
        {
            using var container = new Container();
            container.Bind<Dependency1>().AsCached();
            container.Resolve<Dependency1>();
            var instances = new object[objectCount];
            for (var i = 0; i < instances.Length; i++)
                instances[i] = new TargetWith1Dependency();

            IEnumerable<object> targets = instances;
            container.Inject(targets);

            Measure.Method(() => container.Inject(targets))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void AddToInjectionQueue_WithExistingObjects(int queueSize)
        {
            var queuedInstances = new object[queueSize];
            for (var i = 0; i < queuedInstances.Length; i++)
                queuedInstances[i] = new object();

            var target = new object();
            Container container = null;

            Measure.Method(() => container.AddToInjectionQueue(target))
                .SetUp(() =>
                {
                    container = new Container();
                    container.AddToInjectionQueue(queuedInstances);
                })
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("AddToInjectionQueue", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [Performance]
        public void AddToInjectionQueue_WithPreparedArray(int objectCount)
        {
            var instances = new object[objectCount];
            for (var i = 0; i < instances.Length; i++)
                instances[i] = new object();

            Container container = null;

            Measure.Method(() => container.AddToInjectionQueue(instances))
                .SetUp(() => container = new Container())
                .CleanUp(() => container.Dispose())
                .SampleGroup(new SampleGroup("AddToInjectionQueue", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Inject_FromComponentOnConsumer_AsTransient()
        {
            using var container = new Container();
            container.Bind<InjectComponent>().FromComponentOnConsumer().AsTransient();
            var gameObject = new GameObject("Consumer");
            var consumer = gameObject.AddComponent<ComponentConsumer>();
            gameObject.AddComponent<InjectComponent>();
            container.Inject(consumer);

            Measure.Method(() => container.Inject(consumer))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(gameObject);
        }

        [Test, Performance]
        public void Inject_FromNewComponentOnConsumer_AsTransient()
        {
            using var container = new Container();
            container.Bind<InjectComponent>().FromNewComponentOnConsumer().AsTransient();
            GameObject gameObject = null;
            ComponentConsumer consumer = null;

            Measure.Method(() => container.Inject(consumer))
                .SetUp(() =>
                {
                    gameObject = new GameObject("Consumer");
                    consumer = gameObject.AddComponent<ComponentConsumer>();
                })
                .CleanUp(() => Object.DestroyImmediate(gameObject))
                .WarmupCount(5)
                .MeasurementCount(30)
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();
        }

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(16)]
        [Performance]
        public void Inject_FromComponentInParents_AsTransient(int depth)
        {
            using var container = new Container();
            container.Bind<InjectComponent>().FromComponentInParents().AsTransient();
            var root = new GameObject("Root");
            root.AddComponent<InjectComponent>();
            var leaf = root;

            for (var i = 0; i < depth; i++)
            {
                var child = new GameObject("Child");
                child.transform.SetParent(leaf.transform);
                leaf = child;
            }

            var consumer = leaf.AddComponent<ComponentConsumer>();
            container.Inject(consumer);

            Measure.Method(() => container.Inject(consumer))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [Performance]
        public void Inject_FromComponentInChildren_AsTransient_TargetLast(int descendantCount)
        {
            using var container = new Container();
            container.Bind<InjectComponent>().FromComponentInChildren().AsTransient();
            var root = new GameObject("Consumer");
            var consumer = root.AddComponent<ComponentConsumer>();
            GameObject target = null;

            for (var i = 0; i < descendantCount; i++)
            {
                target = new GameObject("Child");
                target.transform.SetParent(root.transform);
            }

            target.AddComponent<InjectComponent>();
            container.Inject(consumer);

            Measure.Method(() => container.Inject(consumer))
                .SampleGroup(new SampleGroup("Inject", SampleUnit.Nanosecond))
                .Run();

            Object.DestroyImmediate(root);
        }

        public sealed class InjectComponent : MonoBehaviour { }

        public sealed class ComponentConsumer : MonoBehaviour
        {
            [Inject]
            private void Construct(InjectComponent dependency) { }
        }

        private sealed class PlainTarget { }

        private class TargetWith1Dependency
        {
            [Inject]
            private void Construct(Dependency1 dependency) { }
        }

        private sealed class TargetWith3Dependencies
        {
            [Inject]
            private void Construct(Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3) { }
        }

        private sealed class TargetWith10Dependencies
        {
            [Inject]
            private void Construct(
                Dependency1 dependency1, Dependency2 dependency2, Dependency3 dependency3,
                Dependency4 dependency4, Dependency5 dependency5, Dependency6 dependency6,
                Dependency7 dependency7, Dependency8 dependency8, Dependency9 dependency9,
                Dependency10 dependency10) { }
        }

        private sealed class ChainTarget<T>
        {
            [Inject]
            private void Construct(T dependency) { }
        }

        private class Inherited1 : TargetWith1Dependency { }
        private class Inherited2 : Inherited1 { }
        private class Inherited3 : Inherited2 { }
        private class Inherited4 : Inherited3 { }
        private class Inherited5 : Inherited4 { }
        private class Inherited6 : Inherited5 { }
        private class Inherited7 : Inherited6 { }
        private class Inherited8 : Inherited7 { }
        private class Inherited9 : Inherited8 { }
        private class Inherited10 : Inherited9 { }
        private class Inherited11 : Inherited10 { }
        private class Inherited12 : Inherited11 { }
        private class Inherited13 : Inherited12 { }
        private class Inherited14 : Inherited13 { }
        private class Inherited15 : Inherited14 { }
        private sealed class Inherited16 : Inherited15 { }

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

        private sealed class Chain2
        {
            [Preserve]
            public Chain2(Dependency1 dependency) { }
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
