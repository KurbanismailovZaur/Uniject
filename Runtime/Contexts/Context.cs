using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

namespace Uniject.Contexts
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] protected List<MonoInstaller> _installers;
        [SerializeField] protected List<MonoBehaviour> _injectTargets;
        [SerializeField] protected List<GameObjectContext> _gameObjectContexts;
        [SerializeField] protected Transform ParentTransformForGameObjects;
        
        public Container Container { get; protected set; } = new Container();
        public bool IsInstalled { get; protected set; }
        public bool IsBuilded { get; protected set; }

        protected void Awake()
        {
            Container.ParentTransformForGameObjects = ParentTransformForGameObjects;
            Container.Context = this;
        }

        public virtual void Install()
        {
            if (IsInstalled)
                return;

            IsInstalled = true;

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

            foreach (var context in _gameObjectContexts)
                context.Install();
        }

        public virtual void Build()
        {
            if (IsBuilded)
                return;

            IsBuilded = true;

            Container.Build();

            foreach (var context in _gameObjectContexts)
                context.Build();
        }

        protected virtual void OnDestroy()
        {
            if (IsBuilded)
                Container.Dispose();
        }
    }
}
