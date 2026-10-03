namespace Uniject.Installers
{
    public class SignalBusInstaller : MonoInstaller
    {
        public override void Install(Container container)
        {
            container.Bind<SignalBus>()
                .AsCached()
                .DisposeWithContainer();
        }
    }
}
