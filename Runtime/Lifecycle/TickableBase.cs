using System;
using Uniject.Collections;

namespace Uniject.Lifecycle
{
    public abstract class TickableBase<T>
    {
        private protected readonly OrderedSet<T> _tickables = new();
        private protected readonly OrderedSet<T> _tickablesPendingAdd = new();
        private protected readonly OrderedSet<T> _tickablesPendingRemove = new();
        protected bool _isTicking;

        public void Register(T tickable)
        {
            if (tickable == null)
                throw new ArgumentNullException(nameof(tickable));

            if (_tickablesPendingRemove.Remove(tickable))
                return;

            if (_tickables.Contains(tickable) || _tickablesPendingAdd.Contains(tickable))
                throw new ArgumentException($"Tickable is already registered.", nameof(tickable));

            if (_isTicking)
                _tickablesPendingAdd.Add(tickable);
            else
                _tickables.Add(tickable);
        }

        public void Unregister(T tickable)
        {
            if (tickable == null)
                throw new ArgumentNullException(nameof(tickable));

            if (_tickablesPendingAdd.Remove(tickable))
                return;

            if (!_tickables.Contains(tickable) || _tickablesPendingRemove.Contains(tickable))
                throw new ArgumentException($"Tickable is not registered.", nameof(tickable));

            if (_isTicking)
                _tickablesPendingRemove.Add(tickable);
            else
                _tickables.Remove(tickable);
        }

        protected void AddQueuedTickables()
        {
            if (_tickablesPendingAdd.Count <= 0)
                return;

            foreach (var tickable in _tickablesPendingAdd)
                _tickables.Add(tickable);
            
            _tickablesPendingAdd.Clear();
        }

        protected void RemoveQueuedTickables()
        {
            if (_tickablesPendingRemove.Count == 0)
                return;

            foreach (var tickable in _tickablesPendingRemove)
                _tickables.Remove(tickable);

            _tickablesPendingRemove.Clear();
        }

        public abstract void Tick();
    }
}
