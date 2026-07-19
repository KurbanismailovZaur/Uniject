using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Installers
{
    public class SceneLoaderInstaller : MonoInstaller
    {
        public override void Install(Container container)
        {
            container.Bind<SceneLoader>().AsCached();
        }
    }
}