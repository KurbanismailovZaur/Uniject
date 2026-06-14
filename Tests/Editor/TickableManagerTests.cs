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
        private readonly List<GameObject> _gameObjects = new();

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

        private sealed class TestLateTickable : ILateTickable
        {
            public int LateTicksCount { get; private set; }

            public void LateTick() => LateTicksCount++;
        }

        private sealed class TestFixedTickable : IFixedTickable
        {
            public int FixedTicksCount { get; private set; }

            public void FixedTick() => FixedTicksCount++;
        }

        private sealed class TestMultiTickable : ITickable, ILateTickable, IFixedTickable
        {
            public int TicksCount { get; private set; }
            public int LateTicksCount { get; private set; }
            public int FixedTicksCount { get; private set; }

            public void Tick() => TicksCount++;

            public void LateTick() => LateTicksCount++;

            public void FixedTick() => FixedTicksCount++;
        }

        private sealed class NotTickable { }

        [TearDown]
        public void TearDown()
        {
            foreach (var gameObject in _gameObjects)
                UnityEngine.Object.DestroyImmediate(gameObject);

            _gameObjects.Clear();
        }

        private TickableManager CreateTrackedManager()
        {
            var gameObject = new GameObject("TickableManager");
            _gameObjects.Add(gameObject);
            return gameObject.AddComponent<TickableManager>();
        }

        private static void InvokeUnityMessage(TickableManager manager, string methodName)
        {
            var method = typeof(TickableManager).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            try
            {
                method.Invoke(manager, null);
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        private static void Update(TickableManager manager) => InvokeUnityMessage(manager, "Update");

        private static void LateUpdate(TickableManager manager) => InvokeUnityMessage(manager, "LateUpdate");

        private static void FixedUpdate(TickableManager manager) => InvokeUnityMessage(manager, "FixedUpdate");

        [Test]
        public void Update_WhenTickableIsRegistered_CallsTick()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);
            Update(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_WhenTickableIsUnregistered_DoesNotCallTick()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);
            manager.UnregisterTickable(tickable);

            Update(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Register_WhenTickableIsAlreadyRegistered_ThrowsArgumentException()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestTickable();

            manager.RegisterTickable(tickable);

            Assert.That(() => manager.RegisterTickable(tickable), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Unregister_WhenTickableIsNotRegistered_ThrowsArgumentException()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestTickable();

            Assert.That(() => manager.UnregisterTickable(tickable), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Update_WhenTickableRegistersAnotherTickable_DoesNotCallRegisteredTickableInSameUpdate()
        {
            var manager = CreateTrackedManager();
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
            var manager = CreateTrackedManager();
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
            var manager = CreateTrackedManager();
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
            var manager = CreateTrackedManager();
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
            var manager = CreateTrackedManager();
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

        [Test]
        public void LateUpdate_WhenLateTickableIsRegistered_CallsLateTick()
        {
            var manager = CreateTrackedManager();
            var lateTickable = new TestLateTickable();

            manager.RegisterLateTickable(lateTickable);
            LateUpdate(manager);

            Assert.That(lateTickable.LateTicksCount, Is.EqualTo(1));
        }

        [Test]
        public void FixedUpdate_WhenFixedTickableIsRegistered_CallsFixedTick()
        {
            var manager = CreateTrackedManager();
            var fixedTickable = new TestFixedTickable();

            manager.RegisterFixedTickable(fixedTickable);
            FixedUpdate(manager);

            Assert.That(fixedTickable.FixedTicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Register_WhenObjectImplementsAllTickableInterfaces_RegistersAllPhases()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestMultiTickable();

            manager.Register(tickable);

            Update(manager);
            LateUpdate(manager);
            FixedUpdate(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(1));
            Assert.That(tickable.LateTicksCount, Is.EqualTo(1));
            Assert.That(tickable.FixedTicksCount, Is.EqualTo(1));
        }

        [Test]
        public void Unregister_WhenObjectImplementsAllTickableInterfaces_UnregistersAllPhases()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestMultiTickable();

            manager.Register(tickable);
            manager.Unregister(tickable);

            Update(manager);
            LateUpdate(manager);
            FixedUpdate(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(0));
            Assert.That(tickable.LateTicksCount, Is.EqualTo(0));
            Assert.That(tickable.FixedTicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Register_WhenObjectDoesNotImplementAnyTickableInterface_DoesNothing()
        {
            var manager = CreateTrackedManager();

            Assert.That(() => manager.Register(new NotTickable()), Throws.Nothing);
        }

        [Test]
        public void Unregister_WhenObjectDoesNotImplementAnyTickableInterface_DoesNothing()
        {
            var manager = CreateTrackedManager();

            Assert.That(() => manager.Unregister(new NotTickable()), Throws.Nothing);
        }

        [Test]
        public void Register_WhenObjectIsNull_ThrowsArgumentNullException()
        {
            var manager = CreateTrackedManager();

            Assert.That(() => manager.Register(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Unregister_WhenObjectIsNull_ThrowsArgumentNullException()
        {
            var manager = CreateTrackedManager();

            Assert.That(() => manager.Unregister(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Register_WhenObjectIsAlreadyRegistered_ThrowsArgumentException()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestMultiTickable();

            manager.Register(tickable);

            Assert.That(() => manager.Register(tickable), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Register_WhenObjectHasAlreadyRegisteredInterface_DoesNotPartiallyRegisterOtherInterfaces()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestMultiTickable();

            manager.RegisterLateTickable(tickable);

            Assert.That(() => manager.Register(tickable), Throws.TypeOf<ArgumentException>());

            Update(manager);
            LateUpdate(manager);
            FixedUpdate(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(0));
            Assert.That(tickable.LateTicksCount, Is.EqualTo(1));
            Assert.That(tickable.FixedTicksCount, Is.EqualTo(0));
        }

        [Test]
        public void Unregister_WhenObjectHasUnregisteredInterface_DoesNotPartiallyUnregisterOtherInterfaces()
        {
            var manager = CreateTrackedManager();
            var tickable = new TestMultiTickable();

            manager.RegisterTickable(tickable);

            Assert.That(() => manager.Unregister(tickable), Throws.TypeOf<ArgumentException>());

            Update(manager);
            LateUpdate(manager);
            FixedUpdate(manager);

            Assert.That(tickable.TicksCount, Is.EqualTo(1));
            Assert.That(tickable.LateTicksCount, Is.EqualTo(0));
            Assert.That(tickable.FixedTicksCount, Is.EqualTo(0));
        }
    }
}
