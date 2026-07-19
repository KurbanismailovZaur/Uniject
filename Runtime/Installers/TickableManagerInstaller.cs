using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Installers
{
    public class TickableManagerInstaller : MonoInstaller
    {
        public override void Install(Container container)
        {
            var tickableManager = gameObject.AddComponent<TickableManager>();
            container.Bind<TickableManager>().FromInstance(tickableManager).AsCached();
        }
    }
}