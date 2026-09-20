using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Profiling;

namespace Uniject.Tests.Performance.Memory
{
    /// <summary>Managed allocation events per operation on the calling thread, excluding setup, cleanup and reporting.</summary>
    internal static class AllocationMeasurement
    {
        public static double Run(
            string name,
            Action action,
            Action setUp = null,
            Action cleanUp = null,
            int iterations = 100,
            int measurements = 10,
            int warmup = 5,
            int operationsPerIteration = 1,
            bool expectZero = false)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (iterations <= 0 || measurements <= 0 || warmup < 0 || operationsPerIteration <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Invalid measurement counts.");

#if !ENABLE_PROFILER
            Assert.Ignore("GC.Alloc measurements require the Unity Editor or a Development Player with profiling support.");
#endif

            using (var recorder = new ProfilerRecorder(ProfilerCategory.Internal, "GC.Alloc", 1,
                       ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
            {
                EnsureRecorderSupported(recorder);
                var group = new SampleGroup(name + ".AllocationsPerOperation", SampleUnit.Undefined);

                for (var i = 0; i < warmup; i++)
                {
                    try
                    {
                        setUp?.Invoke();
                        action();
                    }
                    finally
                    {
                        cleanUp?.Invoke();
                    }
                }

                double total = 0;
                double maximum = 0;
                for (var sample = 0; sample < measurements; sample++)
                {
                    long allocations = 0;
                    for (var iteration = 0; iteration < iterations; iteration++)
                    {
                        try
                        {
                            setUp?.Invoke();
                            allocations += RecordAllocations(recorder, action);
                        }
                        finally
                        {
                            cleanUp?.Invoke();
                        }
                    }

                    var allocationsPerOperation = (double)allocations / iterations / operationsPerIteration;
                    Measure.Custom(group, allocationsPerOperation);
                    total += allocationsPerOperation;
                    maximum = Math.Max(maximum, allocationsPerOperation);
                }

                if (expectZero)
                    Assert.That(maximum, Is.Zero, name + " allocated after warmup.");

                return total / measurements;
            }
        }

        private static void EnsureRecorderSupported(ProfilerRecorder recorder)
        {
            Assert.That(recorder.Valid, Is.True, "The Unity GC.Alloc profiler marker is unavailable.");

            // Warm the measurement path before checking it. Never turn an unavailable recorder into a false zero.
            Action allocate = AllocateCalibrationPayload;
            RecordAllocations(recorder, allocate);
            var allocations = RecordAllocations(recorder, allocate);
            Assert.That(allocations, Is.GreaterThanOrEqualTo(1),
                "The Unity GC.Alloc recorder did not observe the calibration allocation.");
        }

        private static long RecordAllocations(ProfilerRecorder recorder, Action action)
        {
            recorder.Reset();
            recorder.Start();
            try
            {
                action();
            }
            finally
            {
                // Stop publishes the aggregate immediately, without waiting for the next Unity frame.
                recorder.Stop();
            }

            // SumAllSamplesInFrame stores all events in one sample. Its Count is events, not bytes.
            return recorder.Count == 0 ? 0 : recorder.GetSample(0).Count;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AllocateCalibrationPayload()
        {
            GC.KeepAlive(new byte[1024]);
        }
    }
}
