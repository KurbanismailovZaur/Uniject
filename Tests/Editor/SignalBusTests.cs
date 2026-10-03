using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Uniject.Tests
{
    public class SignalBusTests
    {
        private readonly struct ValueSignal
        {
            public int Value { get; }

            public ValueSignal(int value) => Value = value;
        }

        private sealed class ReferenceSignal
        {
            public int Value { get; }

            public ReferenceSignal(int value) => Value = value;
        }

        private sealed class ConstructedSignal
        {
            public static int InstancesCount { get; set; }

            public ConstructedSignal() => InstancesCount++;
        }

        private interface ISignal { }
        private class BaseSignal { }
        private sealed class DerivedSignal : BaseSignal, ISignal { }

        [Test]
        public void Fire_ValueSignal_DeliversPayload()
        {
            using var bus = new SignalBus();
            var received = new List<int>();
            bus.Subscribe<ValueSignal>(signal => received.Add(signal.Value));

            bus.Fire(new ValueSignal(42));

            Assert.That(received, Is.EqualTo(new[] { 42 }));
        }

        [Test]
        public void Fire_ReferenceSignal_DeliversSameInstance()
        {
            using var bus = new SignalBus();
            var signal = new ReferenceSignal(42);
            ReferenceSignal received = null;
            bus.Subscribe<ReferenceSignal>(value => received = value);

            bus.Fire(signal);

            Assert.That(received, Is.SameAs(signal));
            Assert.That(received.Value, Is.EqualTo(42));
        }

        [Test]
        public void Fire_WithoutArgument_CreatesNewSignalForEachCall()
        {
            using var bus = new SignalBus();
            ConstructedSignal.InstancesCount = 0;
            var received = new List<ConstructedSignal>();
            bus.Subscribe<ConstructedSignal>(received.Add);

            bus.Fire<ConstructedSignal>();
            bus.Fire<ConstructedSignal>();

            Assert.That(ConstructedSignal.InstancesCount, Is.EqualTo(2));
            Assert.That(received.Count, Is.EqualTo(2));
            Assert.That(received[0], Is.Not.Null);
            Assert.That(received[1], Is.Not.SameAs(received[0]));
        }

        [Test]
        public void Fire_WithoutArgumentForValueSignal_DeliversDefaultValue()
        {
            using var bus = new SignalBus();
            var received = new List<int>();
            bus.Subscribe<ValueSignal>(signal => received.Add(signal.Value));

            bus.Fire<ValueSignal>();

            Assert.That(received, Is.EqualTo(new[] { 0 }));
        }

        [Test]
        public void Fire_DerivedSignal_UsesDeclaredGenericTypeOnly()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            var signal = new DerivedSignal();
            bus.Subscribe<DerivedSignal>(_ => received.Add("derived"));
            bus.Subscribe<BaseSignal>(_ => received.Add("base"));
            bus.Subscribe<ISignal>(_ => received.Add("interface"));

            bus.Fire(signal);
            BaseSignal baseSignal = signal;
            bus.Fire(baseSignal);
            bus.Fire<ISignal>(signal);

            Assert.That(received, Is.EqualTo(new[] { "derived", "base", "interface" }));
        }

        [Test]
        public void Unsubscribe_DuplicateHandler_RemovesLastOccurrenceAndPreservesOrder()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            Action<int> first = _ => received.Add("first");
            Action<int> second = _ => received.Add("second");
            bus.Subscribe(first);
            bus.Subscribe(second);
            bus.Subscribe(first);

            bus.Fire(1);

            Assert.That(received, Is.EqualTo(new[] { "first", "second", "first" }));

            received.Clear();
            bus.Unsubscribe(first);
            bus.Fire(2);

            Assert.That(received, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Fire_WithoutSubscribersAndAfterMissingUnsubscribe_DoesNothing()
        {
            using var bus = new SignalBus();
            Action<int> handler = _ => Assert.Fail("An absent handler must not be called.");

            Assert.That(() =>
            {
                bus.Fire(1);
                bus.Unsubscribe(handler);
                bus.Unsubscribe(handler);
                bus.Fire<int>();
            }, Throws.Nothing);
        }

        [Test]
        public void Unsubscribe_LastHandler_AllowsSubscribingAgainWithoutOldHandler()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            Action<int> previous = _ => received.Add("previous");
            bus.Subscribe(previous);
            bus.Unsubscribe(previous);
            bus.Unsubscribe(previous);

            bus.Fire(1);
            bus.Subscribe<int>(_ => received.Add("current"));
            bus.Fire(2);

            Assert.That(received, Is.EqualTo(new[] { "current" }));
        }

        [Test]
        public void Subscribe_DuringFire_AffectsNextDispatchOnly()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            var subscribed = false;
            bus.Subscribe<int>(_ =>
            {
                received.Add("first");
                if (subscribed)
                    return;

                subscribed = true;
                bus.Subscribe<int>(value => received.Add("added"));
            });
            bus.Subscribe<int>(_ => received.Add("second"));

            bus.Fire(1);

            Assert.That(received, Is.EqualTo(new[] { "first", "second" }));

            received.Clear();
            bus.Fire(2);

            Assert.That(received, Is.EqualTo(new[] { "first", "second", "added" }));
        }

        [Test]
        public void Unsubscribe_DuringFire_PreservesCurrentDispatch()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            Action<int> second = _ => received.Add("second");
            Action<int> first = null;
            first = _ =>
            {
                received.Add("first");
                bus.Unsubscribe(first);
                bus.Unsubscribe(second);
            };
            bus.Subscribe(first);
            bus.Subscribe(second);
            bus.Subscribe<int>(_ => received.Add("third"));

            bus.Fire(1);

            Assert.That(received, Is.EqualTo(new[] { "first", "second", "third" }));

            received.Clear();
            bus.Fire(2);

            Assert.That(received, Is.EqualTo(new[] { "third" }));
        }

        [Test]
        public void Fire_NestedDispatch_UsesChangedSubscriptionsAndPreservesOuterDispatch()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            Action<int> second = value => received.Add($"second:{value}");
            Action<int> added = value => received.Add($"added:{value}");
            bus.Subscribe<int>(value =>
            {
                received.Add($"first:{value}");
                if (value != 1)
                    return;

                bus.Unsubscribe(second);
                bus.Subscribe(added);
                bus.Fire(2);
            });
            bus.Subscribe(second);

            bus.Fire(1);

            Assert.That(received, Is.EqualTo(new[] { "first:1", "first:2", "added:2", "second:1" }));
        }

        [Test]
        public void Fire_WhenHandlerThrows_PropagatesSameExceptionAndStopsDispatch()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            var expected = new InvalidOperationException("Handler failed.");
            bus.Subscribe<int>(_ => received.Add("first"));
            bus.Subscribe<int>(_ => throw expected);
            bus.Subscribe<int>(_ => received.Add("last"));

            var actual = Assert.Throws<InvalidOperationException>(() => bus.Fire(1));

            Assert.That(actual, Is.SameAs(expected));
            Assert.That(received, Is.EqualTo(new[] { "first" }));
        }

        [Test]
        public void Fire_SeparateBuses_HaveIndependentSubscriptions()
        {
            using var first = new SignalBus();
            using var second = new SignalBus();
            var received = new List<string>();
            first.Subscribe<int>(value => received.Add($"first:{value}"));
            second.Subscribe<int>(value => received.Add($"second:{value}"));

            first.Fire(1);
            second.Fire(2);

            Assert.That(received, Is.EqualTo(new[] { "first:1", "second:2" }));
        }

        [Test]
        public void Dispose_PreventsSubscribeAndBothFireOverloadsWithoutConstructingSignal()
        {
            using var bus = new SignalBus();
            ConstructedSignal.InstancesCount = 0;
            var received = 0;
            bus.Subscribe<ConstructedSignal>(_ => received++);
            bus.Dispose();

            Assert.That(() => bus.Subscribe<int>(_ => { }), Throws.TypeOf<ObjectDisposedException>());
            Assert.That(() => bus.Fire(1), Throws.TypeOf<ObjectDisposedException>());
            Assert.That(() => bus.Fire<ConstructedSignal>(), Throws.TypeOf<ObjectDisposedException>());
            Assert.That(ConstructedSignal.InstancesCount, Is.Zero);
            Assert.That(received, Is.Zero);
        }

        [Test]
        public void Dispose_RepeatedDisposeAndUnsubscribe_AreSafe()
        {
            using var bus = new SignalBus();
            Action<int> handler = _ => { };
            bus.Subscribe(handler);
            bus.Dispose();

            Assert.That(() =>
            {
                bus.Unsubscribe(handler);
                bus.Unsubscribe(handler);
                bus.Dispose();
            }, Throws.Nothing);
        }

        [Test]
        public void Dispose_DuringFire_CompletesCurrentDispatchAndRejectsNestedDispatch()
        {
            using var bus = new SignalBus();
            var received = new List<string>();
            bus.Subscribe<int>(_ =>
            {
                received.Add("first");
                bus.Dispose();
                Assert.That(() => bus.Fire(2), Throws.TypeOf<ObjectDisposedException>());
            });
            bus.Subscribe<int>(_ => received.Add("second"));

            bus.Fire(1);

            Assert.That(received, Is.EqualTo(new[] { "first", "second" }));
            Assert.That(() => bus.Fire(3), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void Resolve_CachedBinding_ReturnsSharedBusDisposedWithContainer()
        {
            using var container = new Container();
            container.Bind<SignalBus>().AsCached().DisposeWithContainer();
            var subscriberBus = container.Resolve<SignalBus>();
            var publisherBus = container.Resolve<SignalBus>();
            var received = new List<int>();
            subscriberBus.Subscribe<int>(received.Add);

            publisherBus.Fire(42);

            Assert.That(publisherBus, Is.SameAs(subscriberBus));
            Assert.That(received, Is.EqualTo(new[] { 42 }));

            container.Dispose();

            Assert.That(() => publisherBus.Fire(43), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void Resolve_ParentBinding_SharesBusOwnedByParent()
        {
            using var parent = new Container();
            parent.Bind<SignalBus>().AsCached().DisposeWithContainer();
            using var child = new Container(parent);
            var childBus = child.Resolve<SignalBus>();
            var parentBus = parent.Resolve<SignalBus>();
            var received = new List<int>();
            parentBus.Subscribe<int>(received.Add);

            childBus.Fire(1);
            child.Dispose();
            parentBus.Fire(2);

            Assert.That(childBus, Is.SameAs(parentBus));
            Assert.That(received, Is.EqualTo(new[] { 1, 2 }));

            parent.Dispose();

            Assert.That(() => childBus.Fire(3), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void Resolve_LocalBinding_UsesIndependentBusOwnedByChild()
        {
            using var parent = new Container();
            parent.Bind<SignalBus>().AsCached().DisposeWithContainer();
            using var child = new Container(parent);
            child.Bind<SignalBus>().AsCached().DisposeWithContainer();
            var parentBus = parent.Resolve<SignalBus>();
            var childBus = child.Resolve<SignalBus>();
            var received = new List<string>();
            parentBus.Subscribe<int>(value => received.Add($"parent:{value}"));
            childBus.Subscribe<int>(value => received.Add($"child:{value}"));

            childBus.Fire(1);
            parentBus.Fire(2);
            child.Dispose();
            parentBus.Fire(3);

            Assert.That(childBus, Is.Not.SameAs(parentBus));
            Assert.That(received, Is.EqualTo(new[] { "child:1", "parent:2", "parent:3" }));
            Assert.That(() => childBus.Fire(4), Throws.TypeOf<ObjectDisposedException>());
        }
    }
}
