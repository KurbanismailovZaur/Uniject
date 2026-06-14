using System;
using Uniject.Collections;
using UnityEngine;

namespace Uniject.Lifecycle
{
    public class TickableManager : MonoBehaviour
    {
        private readonly OrderedSet<ITickable> _tickables = new();
        private readonly OrderedSet<ITickable> _tickablesPendingAdd = new();
        private readonly OrderedSet<ITickable> _tickablesPendingRemove = new();
        private bool _isTicking;

        public void Register(ITickable tickable)
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

        public void Unregister(ITickable tickable)
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

        private void Update()
        {
            _isTicking = true;

            try
            {
                foreach (var tickable in _tickables)
                {
                    if (!_tickablesPendingRemove.Contains(tickable))
                        tickable.Tick();
                }
            }
            finally
            {
                _isTicking = false;

                RemoveQueuedTickables();
                AddQueuedTickables();
            }
        }

        private void AddQueuedTickables()
        {
            if (_tickablesPendingAdd.Count <= 0)
                return;

            foreach (var tickable in _tickablesPendingAdd)
                _tickables.Add(tickable);
            
            _tickablesPendingAdd.Clear();
        }

        private void RemoveQueuedTickables()
        {
            if (_tickablesPendingRemove.Count == 0)
                return;

            foreach (var tickable in _tickablesPendingRemove)
                _tickables.Remove(tickable);

            _tickablesPendingRemove.Clear();
        }
    }
}
