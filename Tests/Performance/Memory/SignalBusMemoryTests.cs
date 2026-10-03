using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.Memory
{
    public class SignalBusMemoryTests
    {
        [Test, Performance]
        public void Fire_ValueSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ValueSignal>(new Listener().OnValueSignal);
            var signal = new ValueSignal(42);

            AllocationMeasurement.Run("SignalBus.Fire.ValueSignal",
                () => bus.Fire(signal), expectZero: true);
        }

        [Test, Performance]
        public void Fire_PreallocatedReferenceSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ReferenceSignal>(new Listener().OnReferenceSignal);
            var signal = new ReferenceSignal();

            AllocationMeasurement.Run("SignalBus.Fire.PreallocatedReferenceSignal",
                () => bus.Fire(signal), expectZero: true);
        }

        [Test, Performance]
        public void Fire_WithoutArgument_CreatesReferenceSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ReferenceSignal>(new Listener().OnReferenceSignal);

            // Record constructor allocations separately from dispatching an existing instance.
            AllocationMeasurement.Run("SignalBus.Fire.NewReferenceSignal",
                () => bus.Fire<ReferenceSignal>());
        }

        [Test, Performance]
        public void SubscribeUnsubscribe_WarmCycle([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ValueSignal>(new Listener().OnValueSignal);
            var target = new Listener();
            Action<ValueSignal> handler = target.OnValueSignal;
            bus.Subscribe(handler);
            bus.Unsubscribe(handler);

            // Reuse the delegate; record multicast-delegate changes without accumulating handlers.
            AllocationMeasurement.Run("SignalBus.SubscribeUnsubscribe.WarmCycle", () =>
            {
                bus.Subscribe(handler);
                bus.Unsubscribe(handler);
            });

            bus.Fire(new ValueSignal(42));
            Assert.That(target.LastValue, Is.Zero);
        }

        private readonly struct ValueSignal
        {
            public int Value { get; }

            public ValueSignal(int value) => Value = value;
        }

        private sealed class ReferenceSignal
        {
            public int Value { get; }

            [Preserve]
            public ReferenceSignal() => Value = 42;
        }

        private sealed class Listener
        {
            public int LastValue;
            public ReferenceSignal LastReference;

            public void OnValueSignal(ValueSignal signal) => LastValue = signal.Value;

            public void OnReferenceSignal(ReferenceSignal signal) => LastReference = signal;
        }
    }
}
