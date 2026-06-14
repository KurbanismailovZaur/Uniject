using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Tests.Fixtures
{
    public class ClassWithEntryPoint : IEntryPoint, ITickable
    {
        private readonly TickableManager _tickableManager;

        public ClassWithEntryPoint(TickableManager tickableManager)
        {
            _tickableManager = tickableManager;
        }

        public void Run()
        {
            Debug.Log("Class Started!");
            _tickableManager.Register(this);
        }

        public void Tick()
        {
            Debug.Log("Tick!");
        }
    }
}
