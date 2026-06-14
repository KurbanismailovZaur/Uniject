using System;
using UnityEngine;

namespace Uniject.Lifecycle
{
    public class TickableManager : MonoBehaviour
    {
        private readonly Tickable _tickables = new();
        private readonly LateTickable _lateTickables = new();
        private readonly FixedTickable _fixedTickables = new();
        
        public void RegisterTickable(ITickable tickable) => _tickables.Register(tickable);
        
        public void RegisterLateTickable(ILateTickable lateTickable) => _lateTickables.Register(lateTickable);
        
        public void RegisterFixedTickable(IFixedTickable fixedTickable) => _fixedTickables.Register(fixedTickable);

        public void Register(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var tickable = obj as ITickable;
            var lateTickable = obj as ILateTickable;
            var fixedTickable = obj as IFixedTickable;

            if (tickable != null && !_tickables.CanRegister(tickable) ||
                lateTickable != null && !_lateTickables.CanRegister(lateTickable) ||
                fixedTickable != null && !_fixedTickables.CanRegister(fixedTickable))
                throw new ArgumentException("Object has already registered tickable interfaces.", nameof(obj));

            if (tickable != null)
                RegisterTickable(tickable);

            if (lateTickable != null)
                RegisterLateTickable(lateTickable);

            if (fixedTickable != null)
                RegisterFixedTickable(fixedTickable);
        }

        public void UnregisterTickable(ITickable tickable) => _tickables.Unregister(tickable);
        
        public void UnregisterLateTickable(ILateTickable lateTickable) => _lateTickables.Unregister(lateTickable);
        
        public void UnregisterFixedTickable(IFixedTickable fixedTickable) => _fixedTickables.Unregister(fixedTickable);

        public void Unregister(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var tickable = obj as ITickable;
            var lateTickable = obj as ILateTickable;
            var fixedTickable = obj as IFixedTickable;

            if (tickable != null && !_tickables.CanUnregister(tickable) ||
                lateTickable != null && !_lateTickables.CanUnregister(lateTickable) ||
                fixedTickable != null && !_fixedTickables.CanUnregister(fixedTickable))
                throw new ArgumentException("Object has unregistered tickable interfaces.", nameof(obj));

            if (tickable != null)
                UnregisterTickable(tickable);

            if (lateTickable != null)
                UnregisterLateTickable(lateTickable);

            if (fixedTickable != null)
                UnregisterFixedTickable(fixedTickable);
        }

        private void Update() => _tickables.Tick();

        private void LateUpdate() => _lateTickables.Tick();
        
        private void FixedUpdate() => _fixedTickables.Tick();
    }
}
