using Uniject.Installers;

namespace Uniject.Samples.QuickStart
{
    public sealed class GameInstaller : MonoInstaller
    {
        public override void Install(Container container)
        {
            // Share one service instance across consumers.
            container.Bind<ScoreService>().AsCached();

            // Create the entry point and run it at startup.
            container.Bind<GameStartup>().AsEntryPoint();
        }
    }
}
