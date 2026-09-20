using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Memory
{
    [Category("MemoryRetention")]
    public class TickableManagerRetentionTests
    {
        public enum TickPhase { Update, LateUpdate, FixedUpdate }
        public enum Mutation { Remove, AddThenRemove, AddThenUnregisterAfterFlush, RemoveThenThrow }

        private sealed class Listener : ITickable, ILateTickable, IFixedTickable
        {
            public Action OnTick;
            public void Tick() => OnTick?.Invoke();
            public void LateTick() => OnTick?.Invoke();
            public void FixedTick() => OnTick?.Invoke();
        }

        [Test]
        public void Unregister_DoesNotRetainListener()
        {
            var gameObject = new GameObject(nameof(TickableManagerRetentionTests));
            try
            {
                var manager = gameObject.AddComponent<TickableManager>();
                var reference = Register(manager);
                MemoryRetentionAssert.Retained(manager, reference);
                Unregister(manager, reference);

                MemoryRetentionAssert.Collected(manager, reference);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Dispatch_DoesNotRetainProcessedPendingChanges(
            [Values] TickPhase phase, [Values] Mutation mutation)
        {
            var gameObject = new GameObject(nameof(TickableManagerRetentionTests));
            try
            {
                var manager = gameObject.AddComponent<TickableManager>();
                var references = RegisterMutateAndDispatch(manager, phase, mutation);

                MemoryRetentionAssert.Collected(manager, references);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference Register(TickableManager manager)
        {
            var listener = new Listener();
            manager.Register(listener);
            return new WeakReference(listener);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Unregister(TickableManager manager, WeakReference reference)
        {
            manager.Unregister(reference.Target);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] RegisterMutateAndDispatch(
            TickableManager manager, TickPhase phase, Mutation mutation)
        {
            var target = new Listener();
            var actor = new Listener();
            if (mutation == Mutation.Remove || mutation == Mutation.RemoveThenThrow)
                manager.Register(target);

            actor.OnTick = () =>
            {
                if (mutation == Mutation.AddThenRemove || mutation == Mutation.AddThenUnregisterAfterFlush)
                    manager.Register(target);
                if (mutation != Mutation.AddThenUnregisterAfterFlush)
                    manager.Unregister(target);
                manager.Unregister(actor);
                actor.OnTick = null;
                if (mutation == Mutation.RemoveThenThrow)
                    throw new InvalidOperationException("Expected tick failure.");
            };
            manager.Register(actor);
            var method = typeof(TickableManager).GetMethod(
                phase.ToString(), BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var dispatch = (Action)Delegate.CreateDelegate(typeof(Action), manager, method);
            if (mutation == Mutation.RemoveThenThrow)
                Assert.Throws<InvalidOperationException>(() => dispatch());
            else
                dispatch();

            if (mutation == Mutation.AddThenUnregisterAfterFlush)
                manager.Unregister(target);
            return new[] { new WeakReference(target), new WeakReference(actor) };
        }
    }
}
