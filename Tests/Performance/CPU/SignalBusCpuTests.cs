using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.CPU
{
    public class SignalBusCpuTests
    {
        [Test, Performance]
        public void Fire_ValueSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ValueSignal>(new Listener().OnValueSignal);
            var signal = new ValueSignal(42);

            Measure.Method(() => bus.Fire(signal))
                .SampleGroup(new SampleGroup("SignalBus.Fire.ValueSignal", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Fire_PreallocatedReferenceSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ReferenceSignal>(new Listener().OnReferenceSignal);
            var signal = new ReferenceSignal();

            Measure.Method(() => bus.Fire(signal))
                .SampleGroup(new SampleGroup("SignalBus.Fire.PreallocatedReferenceSignal", SampleUnit.Nanosecond))
                .Run();
        }

        [Test, Performance]
        public void Fire_WithoutArgument_CreatesReferenceSignal([Values(0, 1, 10, 100)] int subscriberCount)
        {
            using var bus = new SignalBus();
            for (var i = 0; i < subscriberCount; i++)
                bus.Subscribe<ReferenceSignal>(new Listener().OnReferenceSignal);

            // Includes construction, unlike the preallocated-payload measurement.
            Measure.Method(() => bus.Fire<ReferenceSignal>())
                .SampleGroup(new SampleGroup("SignalBus.Fire.NewReferenceSignal", SampleUnit.Nanosecond))
                .Run();
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

            // Reuse the delegate and restore the subscription count after each operation.
            Measure.Method(() =>
                {
                    bus.Subscribe(handler);
                    bus.Unsubscribe(handler);
                })
                .SampleGroup(new SampleGroup("SignalBus.SubscribeUnsubscribe.WarmCycle", SampleUnit.Nanosecond))
                .Run();

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
