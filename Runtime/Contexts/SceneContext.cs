using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public class SceneContext : MonoBehaviour
    {
        [SerializeField] private List<MonoInstaller> _installers;
        [SerializeField] private List<MonoBehaviour> _injectTargets;
        
        public Container Container { get; private set; }
        public bool IsBuilded => Container?.IsBuilded ?? false;

        private void Awake() => Container = new Container();

        private void Start() => Build();

        public void Build()
        {
            if (IsBuilded)
                return;
            
            Container.BindInstance(this);
            Container.Bind<SceneLoader>().AsCached();
            Container.BindInstance(gameObject.AddComponent<TickableManager>());

            foreach (var installer in _installers)
                installer.Install(Container);

            foreach (var target in _injectTargets)
            {
                if (target is InjectTargets injectTargets)
                    Container.AddToInjectionQueue(injectTargets.Targets);
                else
                    Container.AddToInjectionQueue(target);
            }

            Container.Build();
        }

        void OnDestroy()
        {
            if (IsBuilded)
                Container.Dispose();
        }
    }
}
