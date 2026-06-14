using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests
{
    public class TickableManagerTests
    {
        private sealed class TestTickable : ITickable
        {
            private readonly Action _onTick;

            public int TicksCount { get; private set; }

            public TestTickable(Action onTick = null)
            {
                _onTick = onTick;
            }

            public void Tick()
            {
                TicksCount++;
                _onTick?.Invoke();
            }
        }

        private static TickableManager CreateManager()
        {
            var gameObject = new GameObject("TickableManager");
            return gameObject.AddComponent<TickableManager>();
        }

        private static void Update(TickableManager manager)
        {
            var method = typeof(TickableManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(manager, null);
        }

        [Test]
        public void Update_WhenTickableIsRegistered_CallsTick()
        {
            var manager = CreateManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);
            Update(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_WhenTickableIsUnregistered_DoesNotCallTick()
        {
            var manager = CreateManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);
            manager.UnregisterTickable(tickable);

            Update(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Register_WhenTickableIsAlreadyRegistered_ThrowsArgumentException()
        {
            var manager = CreateManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);

            Assert.That(() => manager.RegisterTickable(tickable), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Unregister_WhenTickableIsNotRegistered_ThrowsArgumentException()
        {
            var manager = CreateManager();
            var tickable = new TestTickable();

            Assert.That(() => manager.UnregisterTickable(tickable), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Update_WhenTickableRegistersAnotherTickable_DoesNotCallRegisteredTickableInSameUpdate()
        {
            var manager = CreateManager();
            var registeredDuringTick = new TestTickable();
            var wasRegistered = false;

            var registrar = new TestTickable(() =>
            {
                if (wasRegistered)
                    return;

                wasRegistered = true;
                manager.RegisterTickable(registeredDuringTick);
            });

            manager.RegisterTickable(registrar);

            Update(manager);

            Assert.That(registeredDuringTick.TicksCount, Is.EqualTo(0));

            Update(manager);

            Assert.That(registeredDuringTick.TicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_WhenTickableUnregistersAnotherTickableBeforeItsTurn_DoesNotCallUnregisteredTickable()
        {
            var manager = CreateManager();
            var second = new TestTickable();
            var first = new TestTickable(() => manager.UnregisterTickable(second));

            manager.RegisterTickable(first);
            manager.RegisterTickable(second);

            Update(manager);

            Assert.That(first.TicksCount, Is.EqualTo(1));
            Assert.That(second.TicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Update_WhenTickableUnregistersItself_DoesNotCallItOnNextUpdate()
        {
            var manager = CreateManager();
            var tickable = default(TestTickable);
            tickable = new TestTickable(() => manager.UnregisterTickable(tickable));

            manager.RegisterTickable(tickable);

            Update(manager);
            Update(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_WhenTickableRegistersThenUnregistersAnotherTickable_DoesNotRegisterIt()
        {
            var manager = CreateManager();
            var other = new TestTickable();
            var tickable = new TestTickable(() =>
            {
                manager.RegisterTickable(other);
                manager.UnregisterTickable(other);
            });

            manager.RegisterTickable(tickable);

            Update(manager);
            Update(manager);

            Assert.That(other.TicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Update_WhenTickableUnregistersThenRegistersAnotherTickable_DoesNotUnregisterIt()
        {
            var manager = CreateManager();
            var other = new TestTickable();
            var tickable = new TestTickable(() =>
            {
                manager.UnregisterTickable(other);
                manager.RegisterTickable(other);
            });

            manager.RegisterTickable(tickable);
            manager.RegisterTickable(other);

            Update(manager);
            Update(manager);

            Assert.That(other.TicksCount, Is.EqualTo(2));
        }
    }
}
