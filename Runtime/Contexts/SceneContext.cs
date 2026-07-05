using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public class SceneContext : MonoBehaviour
    {
        [SerializeField] private bool _autoStart;
        [SerializeField] private List<MonoInstaller> _installers;
        [SerializeField] private List<MonoBehaviour> _injectTargets;
        
        public Container Container { get; private set; }

        private void Start()
        {
            if (!_autoStart)
                return;

            Container = new Container();

            var tickableManager = gameObject.AddComponent<TickableManager>();
            Container.BindInstance(tickableManager);

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
    }
}
