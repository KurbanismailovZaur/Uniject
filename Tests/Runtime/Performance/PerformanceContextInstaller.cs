using Uniject.Installers;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    [Preserve]
    public sealed class PerformanceContextInstaller : MonoInstaller
    {
        [Preserve]
        public override void Install(Container container)
        {
            container.BindInstance(new ContextPerformanceTests.PerformanceContextDependency());
        }
    }
}
