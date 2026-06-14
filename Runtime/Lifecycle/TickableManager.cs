using System;
using Uniject.Collections;
using UnityEngine;

namespace Uniject.Lifecycle
{
    public class TickableManager : MonoBehaviour
    {
        private Tickable _tickables = new();
        private LateTickable _lateTickables = new();
        private FixedTickable _fixedTickables = new();
        
        public void RegisterTickable(ITickable tickable) => _tickables.Register(tickable);
        
        public void RegisterLateTickable(ILateTickable lateTickable) => _lateTickables.Register(lateTickable);
        
        public void RegisterFixedTickable(IFixedTickable fixedTickable) => _fixedTickables.Register(fixedTickable);

        public void Register(object obj)
        {
            if (obj is ITickable tickable)
                RegisterTickable(tickable);

            if (obj is ILateTickable lateTickable)
                RegisterLateTickable(lateTickable);

            if (obj is IFixedTickable fixedTickable)
                RegisterFixedTickable(fixedTickable);
        }

        public void UnregisterTickable(ITickable tickable) => _tickables.Unregister(tickable);
        
        public void UnregisterLateTickable(ILateTickable lateTickable) => _lateTickables.Unregister(lateTickable);
        
        public void UnregisterFixedTickable(IFixedTickable fixedTickable) => _fixedTickables.Unregister(fixedTickable);

        public void Unregister(object obj)
        {
            if (obj is ITickable tickable)
                UnregisterTickable(tickable);

            if (obj is ILateTickable lateTickable)
                UnregisterLateTickable(lateTickable);

            if (obj is IFixedTickable fixedTickable)
                UnregisterFixedTickable(fixedTickable);
        }


        private void Update() => _tickables.Tick();

        private void LateUpdate() => _lateTickables.Tick();
        
        private void FixedUpdate() => _fixedTickables.Tick();
    }
}
