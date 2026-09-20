using System;
using Uniject.Attributes;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.Memory
{
    // Shared types keep reflection warmup and direct-construction baselines identical across suites.
    public static class ContainerMemoryFixtures
    {
        public interface IService { }

        public sealed class Service : IService
        {
            [Preserve] public Service() { }
        }

        public sealed class Dependency
        {
            [Preserve] public Dependency() { }
        }

        public sealed class Service1
        {
            public readonly Dependency Dependency;
            [Preserve] public Service1(Dependency dependency) => Dependency = dependency;
        }

        public sealed class Service3
        {
            [Preserve] public Service3(Dependency first, Dependency second, Dependency third) { }
        }

        public sealed class Service10
        {
            [Preserve]
            public Service10(Dependency d1, Dependency d2, Dependency d3, Dependency d4, Dependency d5,
                Dependency d6, Dependency d7, Dependency d8, Dependency d9, Dependency d10) { }
        }

        public sealed class Chain<T>
        {
            public readonly T Dependency;
            [Preserve] public Chain(T dependency) => Dependency = dependency;
        }

        public class InjectionTarget
        {
            public Dependency Dependency { get; private set; }
            [Inject, Preserve] public void Construct(Dependency dependency) => Dependency = dependency;
        }

        public sealed class InheritedInjectionTarget : InjectionTarget { }

        public sealed class InjectionTarget3
        {
            [Inject, Preserve] public void Construct(Dependency d1, Dependency d2, Dependency d3) { }
        }

        public sealed class InjectionTarget10
        {
            [Inject, Preserve]
            public void Construct(Dependency d1, Dependency d2, Dependency d3, Dependency d4, Dependency d5,
                Dependency d6, Dependency d7, Dependency d8, Dependency d9, Dependency d10) { }
        }

        public sealed class ChainInjectionTarget<T>
        {
            public T Dependency { get; private set; }
            [Preserve] public ChainInjectionTarget() { }
            [Inject, Preserve] public void Construct(T dependency) => Dependency = dependency;
        }

        public sealed class Contract<T1, T2, T3>
        {
            [Preserve] public Contract() { }
        }

        public sealed class EntryPoint<T1, T2, T3> : IEntryPoint
        {
            [Preserve] public EntryPoint() { }
            public void Run() { }
        }

        public sealed class DisposableService : IDisposable, IService
        {
            public int DisposeCount { get; private set; }
            [Preserve] public DisposableService() { }
            public void Dispose() => DisposeCount++;
        }

        public sealed class ServiceSource
        {
            public readonly Service Service = new();
            [Preserve] public ServiceSource() { }
        }

        public sealed class ServiceInstaller : IInstaller
        {
            [Preserve] public ServiceInstaller() { }
            public void Install(Container container) => container.Bind<Service>().AsCached();
        }

        public interface IContainerHandle
        {
            Container Container { get; }
        }

        public sealed class ContainerHandle : IContainerHandle
        {
            public Container Container { get; }
            [Preserve] public ContainerHandle(Container container) => Container = container;
        }

        public static Type ServiceType(int dependencyCount) => dependencyCount switch
        {
            0 => typeof(Service),
            1 => typeof(Service1),
            3 => typeof(Service3),
            10 => typeof(Service10),
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyCount))
        };

        public static object CreateInjectionTarget(int dependencyCount) => dependencyCount switch
        {
            0 => new Service(),
            1 => new InjectionTarget(),
            3 => new InjectionTarget3(),
            10 => new InjectionTarget10(),
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyCount))
        };

        public static Func<object> DirectConstructor(int dependencyCount, Dependency dependency) => dependencyCount switch
        {
            0 => () => new Service(),
            1 => () => new Service1(dependency),
            3 => () => new Service3(dependency, dependency, dependency),
            10 => () => new Service10(dependency, dependency, dependency, dependency, dependency,
                dependency, dependency, dependency, dependency, dependency),
            _ => throw new ArgumentOutOfRangeException(nameof(dependencyCount))
        };

        public static Type[] CreateTypes(Type genericType, int count)
        {
            var arguments = new[]
            {
                typeof(object), typeof(string), typeof(Type), typeof(Exception), typeof(Attribute),
                typeof(Delegate), typeof(Container), typeof(IObjectBuilder), typeof(IService), typeof(Service)
            };
            var types = new Type[count];
            for (var i = 0; i < count; i++)
                types[i] = genericType.MakeGenericType(arguments[i / 100], arguments[i / 10 % 10], arguments[i % 10]);
            return types;
        }

        public static Type[] ChainTypes(int objectCount)
        {
            var types = new Type[objectCount];
            types[0] = typeof(Dependency);
            for (var i = 1; i < types.Length; i++)
                types[i] = typeof(Chain<>).MakeGenericType(types[i - 1]);
            return types;
        }

        public static Container[] Hierarchy(Container root, int depth)
        {
            var containers = new Container[depth + 1];
            containers[0] = root;
            for (var i = 1; i < containers.Length; i++)
                containers[i] = new Container(containers[i - 1]);
            return containers;
        }

        public static void DisposeHierarchy(Container[] containers, bool includeRoot = true)
        {
            for (var i = containers.Length - 1; i >= (includeRoot ? 0 : 1); i--)
                containers[i].Dispose();
        }
    }
}
