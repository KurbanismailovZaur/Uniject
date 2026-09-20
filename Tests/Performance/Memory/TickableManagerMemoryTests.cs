using System;
using System.Reflection;
using NUnit.Framework;
using Uniject.Lifecycle;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Uniject.Tests.Performance.Memory
{
    public class TickableManagerMemoryTests
    {
        public enum TickKind
        {
            Tick,
            LateTick,
            FixedTick
        }

        [Test, Performance]
        public void Dispatch_SteadyState([Values] TickKind kind, [Values(0, 1, 100, 1000)] int count)
        {
            using var fixture = new TickFixture(kind);
            for (var i = 0; i < count; i++)
                fixture.Register(new Listener());

            // OrderedSet exposes IEnumerator<T>; record its current enumeration allocation.
            // A zero-allocation assertion would be a runtime change, not a baseline test.
            AllocationMeasurement.Run("TickableManager.Dispatch", fixture.Dispatch);
        }

        [Test, Performance]
        public void Register_FirstAtExistingCount([Values] TickKind kind, [Values(0, 10, 100, 1000)] int count)
        {
            var listeners = CreateListeners(count);
            var target = new Listener();
            TickFixture fixture = null;
            AllocationMeasurement.Run("TickableManager.Register", () => fixture.Register(target),
                setUp: () =>
                {
                    fixture = new TickFixture(kind);
                    foreach (var listener in listeners)
                        fixture.Register(listener);
                }, cleanUp: () => fixture.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void Unregister_ByPosition(
            [Values] TickKind kind, [Values(1, 100, 1000)] int count,
            [Values("First", "Middle", "Last")] string position)
        {
            var listeners = CreateListeners(count);
            var index = position == "First" ? 0 : position == "Middle" ? count / 2 : count - 1;
            TickFixture fixture = null;
            AllocationMeasurement.Run("TickableManager.Unregister", () => fixture.Unregister(listeners[index]),
                setUp: () =>
                {
                    fixture = new TickFixture(kind);
                    foreach (var listener in listeners)
                        fixture.Register(listener);
                }, cleanUp: () => fixture.Dispose(), iterations: 1, expectZero: true);
        }

        [Test, Performance]
        public void RegisterUnregister_WarmCycle([Values] TickKind kind, [Values(0, 100, 1000)] int count)
        {
            using var fixture = new TickFixture(kind);
            foreach (var listener in CreateListeners(count))
                fixture.Register(listener);
            var target = new Listener();
            fixture.Register(target);
            fixture.Unregister(target);

            AllocationMeasurement.Run("TickableManager.WarmRegisterUnregister", () =>
            {
                fixture.Register(target);
                fixture.Unregister(target);
            }, expectZero: true);
        }

        [Test, Performance]
        public void Register_Object_AllInterfaces_FirstAtExistingCount([Values(0, 10, 1000)] int count)
        {
            var listeners = CreateListeners(count);
            var target = new Listener();
            TickFixture fixture = null;
            AllocationMeasurement.Run("TickableManager.RegisterObject.AllInterfaces",
                () => fixture.Manager.Register(target),
                setUp: () =>
                {
                    fixture = new TickFixture(TickKind.Tick);
                    foreach (var listener in listeners)
                        fixture.Manager.Register(listener);
                }, cleanUp: () => fixture.Dispose(), iterations: 1);
        }

        [Test, Performance]
        public void RegisterUnregister_Object_AllInterfaces_WarmCycle()
        {
            using var fixture = new TickFixture(TickKind.Tick);
            var listener = new Listener();
            fixture.Manager.Register(listener);
            fixture.Manager.Unregister(listener);
            AllocationMeasurement.Run("TickableManager.WarmRegisterUnregisterObject.AllInterfaces", () =>
            {
                fixture.Manager.Register(listener);
                fixture.Manager.Unregister(listener);
            }, expectZero: true);
        }

        [Test, Performance]
        public void Dispatch_WithPendingChanges_WarmReversibleCycle(
            [Values] TickKind kind, [Values(1, 10, 100)] int changedCount)
        {
            using var fixture = new TickFixture(kind);
            var current = CreateListeners(changedCount);
            var next = CreateListeners(changedCount);
            var controller = new Listener();
            controller.Callback = () =>
            {
                for (var i = 0; i < current.Length; i++)
                {
                    fixture.Unregister(current[i]);
                    fixture.Register(next[i]);
                }
                var previous = current;
                current = next;
                next = previous;
            };
            fixture.Register(controller);
            foreach (var listener in current)
                fixture.Register(listener);

            Action cycle = () =>
            {
                fixture.Dispatch();
                fixture.Dispatch();
            };
            cycle();
            AllocationMeasurement.Run("TickableManager.Dispatch.PendingChanges", cycle,
                iterations: 20, operationsPerIteration: 2);
        }

        [Test, Performance]
        public void Dispatch_WithCancelledPendingChanges([Values] TickKind kind)
        {
            using var fixture = new TickFixture(kind);
            var existing = new Listener();
            var pending = new Listener();
            var controller = new Listener
            {
                Callback = () =>
                {
                    fixture.Register(pending);
                    fixture.Unregister(pending);
                    fixture.Unregister(existing);
                    fixture.Register(existing);
                }
            };
            fixture.Register(controller);
            fixture.Register(existing);
            fixture.Dispatch();

            AllocationMeasurement.Run("TickableManager.Dispatch.CancelledPendingChanges", fixture.Dispatch);
        }

        private static Listener[] CreateListeners(int count)
        {
            var listeners = new Listener[count];
            for (var i = 0; i < count; i++)
                listeners[i] = new Listener();
            return listeners;
        }

        private sealed class Listener : ITickable, ILateTickable, IFixedTickable
        {
            public Action Callback;

            public void Tick() => Callback?.Invoke();
            public void LateTick() => Callback?.Invoke();
            public void FixedTick() => Callback?.Invoke();
        }

        private sealed class TickFixture : IDisposable
        {
            private readonly GameObject _root;
            private readonly TickKind _kind;
            public TickableManager Manager { get; }
            public Action Dispatch { get; }

            public TickFixture(TickKind kind)
            {
                _kind = kind;
                _root = new GameObject("TickableManagerMemoryTest");
                try
                {
                    Manager = _root.AddComponent<TickableManager>();
                    Manager.enabled = false;
                    var methodName = kind switch
                    {
                        TickKind.Tick => "Update",
                        TickKind.LateTick => "LateUpdate",
                        TickKind.FixedTick => "FixedUpdate",
                        _ => throw new ArgumentOutOfRangeException(nameof(kind))
                    };
                    var method = typeof(TickableManager).GetMethod(methodName,
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method == null)
                        throw new MissingMethodException(typeof(TickableManager).FullName, methodName);
                    // Resolve and bind once: MethodInfo.Invoke would pollute every allocation sample.
                    Dispatch = (Action)Delegate.CreateDelegate(typeof(Action), Manager, method);
                }
                catch
                {
                    Object.DestroyImmediate(_root);
                    throw;
                }
            }

            public void Register(Listener listener)
            {
                switch (_kind)
                {
                    case TickKind.Tick: Manager.RegisterTickable(listener); break;
                    case TickKind.LateTick: Manager.RegisterLateTickable(listener); break;
                    case TickKind.FixedTick: Manager.RegisterFixedTickable(listener); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            public void Unregister(Listener listener)
            {
                switch (_kind)
                {
                    case TickKind.Tick: Manager.UnregisterTickable(listener); break;
                    case TickKind.LateTick: Manager.UnregisterLateTickable(listener); break;
                    case TickKind.FixedTick: Manager.UnregisterFixedTickable(listener); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            public void Dispose() => Object.DestroyImmediate(_root);
        }
    }
}
