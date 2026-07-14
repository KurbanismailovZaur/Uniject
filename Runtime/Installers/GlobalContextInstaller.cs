using Uniject.Lifecycle;

namespace Uniject.Installers
{
    public class GlobalContextInstaller : MonoInstaller
    {
        public override void Install(Container container)
        {
            container.Bind<SceneLoader>().AsCached();
            container.BindInstance(gameObject.AddComponent<TickableManager>());
        }
    }
}