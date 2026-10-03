using System;
using System.Collections.Generic;

namespace Uniject
{
    /// <summary>
    /// Dispatches signals synchronously by their exact generic type. Not thread-safe.
    /// </summary>
    public sealed class SignalBus : IDisposable
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();
        private bool _isDisposed;

        public void Subscribe<T>(Action<T> handler)
        {
            ThrowIfDisposed();

            var signalType = typeof(T);
            _handlers.TryGetValue(signalType, out var handlers);
            _handlers[signalType] = (Action<T>)handlers + handler;
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var signalType = typeof(T);
            if (!_handlers.TryGetValue(signalType, out var handlers))
                return;

            var remainingHandlers = (Action<T>)handlers - handler;
            if (remainingHandlers == null)
                _handlers.Remove(signalType);
            else
                _handlers[signalType] = remainingHandlers;
        }

        public void Fire<T>(T signal)
        {
            ThrowIfDisposed();
            Dispatch(signal);
        }

        public void Fire<T>() where T : new()
        {
            ThrowIfDisposed();
            Dispatch(new T());
        }

        public void Dispose()
        {
            _isDisposed = true;
            _handlers.Clear();
        }

        private void Dispatch<T>(T signal)
        {
            if (_handlers.TryGetValue(typeof(T), out var handlers))
                ((Action<T>)handlers)?.Invoke(signal);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SignalBus));
        }
    }
}
