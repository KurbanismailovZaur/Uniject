using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.Memory
{
    /// <summary>
    /// Diagnostic whole-heap deltas, not allocation counts or deterministic leak assertions.
    /// Run explicitly in an otherwise idle process; other managed allocations can affect the result.
    /// </summary>
    [Explicit("Retained heap diagnostics force full GC and require an otherwise idle process.")]
    public class ManagedFootprintMemoryTests
    {
        private const int OwnerCount = 64;
        private static readonly Type[] ServiceTypes =
        {
            typeof(Service<byte>), typeof(Service<short>), typeof(Service<int>), typeof(Service<long>),
            typeof(Service<float>), typeof(Service<double>), typeof(Service<decimal>), typeof(Service<object>)
        };

        [Test, Performance]
        public void Container_RetainedBytes([Values(0, 1, 8)] int bindingCount,
            [Values(false, true)] bool resolve)
        {
            RecordFootprint("Container", () => CreateContainer(bindingCount, resolve));
        }

        [Test, Performance]
        public void Pool_RetainedBytes([Values(0, 16, 256)] int initialSize)
        {
            RecordFootprint("PoolWithContainer", () => CreatePool(initialSize));
        }

        [Test, Performance]
        public void CollectionPool_RetainedBuffers([Values(16, 256, 4096)] int peakCapacity,
            [Values(false, true)] bool clearAfterPeak)
        {
            RecordFootprint("CollectionPoolAfterPeak", () => CreateCollectionPool(peakCapacity, clearAfterPeak));
        }

        private static void RecordFootprint(string name, Func<IDisposable> create)
        {
            // Warm static metadata and JIT before establishing the baseline.
            for (var i = 0; i < 3; i++)
                create().Dispose();

            var owners = new IDisposable[OwnerCount];
            var group = new SampleGroup(name + ".RetainedBytesPerOwner", SampleUnit.Byte);
            for (var sample = 0; sample < 5; sample++)
            {
                try
                {
                    var before = CollectAndRead();
                    FillOwners(owners, create);
                    var after = CollectAndRead();
                    // Preserve signed deltas: clamping negative noise would bias the estimate upwards.
                    Measure.Custom(group, (double)(after - before) / OwnerCount);
                    GC.KeepAlive(owners);
                }
                finally
                {
                    for (var i = 0; i < owners.Length; i++)
                    {
                        owners[i]?.Dispose();
                        owners[i] = null;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void FillOwners(IDisposable[] owners, Func<IDisposable> create)
        {
            for (var i = 0; i < owners.Length; i++)
                owners[i] = create();
        }

        private static long CollectAndRead()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return GC.GetTotalMemory(true);
        }

        private static Container CreateContainer(int bindingCount, bool resolve)
        {
            var container = new Container();
            for (var i = 0; i < bindingCount; i++)
            {
                container.Bind(ServiceTypes[i]).AsCached();
                if (resolve)
                    container.Resolve(ServiceTypes[i]);
            }

            return container;
        }

        private static Container CreatePool(int initialSize)
        {
            var container = new Container();
            container.BindPool<Service<object>, Pool<Service<object>>>()
                .WithInitialSize(initialSize).FromConstructor().AsCached();
            container.Resolve<Pool<Service<object>>>();
            return container;
        }

        private static CollectionPool CreateCollectionPool(int peakCapacity, bool clearAfterPeak)
        {
            var pool = new CollectionPool();
            // Grow a default-capacity bucket; later small rentals reuse its high-water buffer.
            var list = pool.SpawnList<int>();
            for (var i = 0; i < peakCapacity; i++)
                list.Add(i);
            pool.DespawnList(list);
            var small = pool.SpawnList<int>();
            small.Add(1);
            pool.DespawnList(small);
            if (clearAfterPeak)
                pool.Clear();
            return pool;
        }

        [Preserve]
        public sealed class Service<T>
        {
            [Preserve]
            public Service() { }
        }
    }
}
