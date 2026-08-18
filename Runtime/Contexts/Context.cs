using System;
using System.Collections.Generic;
using Uniject.Components;
using Uniject.Installers;
using UnityEngine;

namespace Uniject.Contexts
{
    public abstract class Context : MonoBehaviour
    {
        [SerializeField] protected bool _useSiblingInstallers = true;
        [SerializeField] protected List<MonoInstaller> _installers = new();
        [SerializeField] protected bool _injectInAllContextGameObjects = true;
        [SerializeField] protected List<MonoBehaviour> _injectTargets = new();
        [SerializeField] protected List<GameObjectContext> _gameObjectContexts = new();
        [SerializeField] protected Transform ParentTransformForGameObjects;
        
        public Container Container { get; protected set; }
        public bool IsInitialized { get; protected set; }
        public bool IsInstalled { get; protected set; }
        public bool IsBuilded { get; protected set; }

        protected abstract void InjectInAllContextGameObjects();

        public virtual void Initialize(Container parentContainer = null)
        {
            if (IsInitialized)
                return;
            
            IsInitialized = true;
            Container = new Container(parentContainer, ParentTransformForGameObjects, this); 

            foreach (var context in _gameObjectContexts)
                context.Initialize(Container);
        }

        public virtual void Install()
        {
            if (IsInstalled)
                return;

            IsInstalled = true;

            var installers = _useSiblingInstallers ? (IList<MonoInstaller>)GetComponents<MonoInstaller>() : _installers;
            
            foreach (var installer in installers)
                installer.Install(Container);               

            if (_injectInAllContextGameObjects)
                InjectInAllContextGameObjects();
            else
            {
                foreach (var target in _injectTargets)
                {
                    if (target is InjectTargets injectTargetsComponent)
                        Container.AddToInjectionQueue(injectTargetsComponent.Targets);
                    else
                        Container.AddToInjectionQueue(target);
                }
            }

            foreach (var context in _gameObjectContexts)
                context.Install();
        }

        protected void InjectMonoBehavioursInHierarchies(
            IReadOnlyList<GameObject> rootGameObjects,
            Transform allowedGameObjectContextRoot = null)
        {
            if (rootGameObjects == null)
                throw new ArgumentNullException(nameof(rootGameObjects));

            var injectTargets = StaticCollections.collectionPool.SpawnList<MonoBehaviour>();
            var monoBehaviours = StaticCollections.collectionPool.SpawnList<MonoBehaviour>();
            var pendingTransforms = StaticCollections.collectionPool.SpawnStack<Transform>();

            foreach (var rootGameObject in rootGameObjects)
            {
                if (rootGameObject == null)
                    continue;

                pendingTransforms.Push(rootGameObject.transform);

                while (pendingTransforms.Count > 0)
                {
                    var currentTransform = pendingTransforms.Pop();

                    if (currentTransform == null)
                        continue;

                    if (currentTransform != allowedGameObjectContextRoot &&
                        currentTransform.TryGetComponent<GameObjectContext>(out _))
                    {
                        continue;
                    }

                    monoBehaviours.Clear();
                    currentTransform.GetComponents(monoBehaviours);
                    injectTargets.AddRange(monoBehaviours);

                    for (var i = currentTransform.childCount - 1; i >= 0; i--)
                        pendingTransforms.Push(currentTransform.GetChild(i));
                }
            }

            StaticCollections.collectionPool.DespawnStack(pendingTransforms);
            StaticCollections.collectionPool.DespawnList(monoBehaviours);

            foreach (var target in injectTargets)
                Container.AddToInjectionQueue(target);

            StaticCollections.collectionPool.DespawnList(injectTargets);
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

        public void Run()
        {
            Initialize();
            Install();
            Build();
        }
    }
}
