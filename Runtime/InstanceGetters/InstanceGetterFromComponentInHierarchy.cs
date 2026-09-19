using System;
using System.Collections.Generic;
using Uniject.Bindings;
using Uniject.Contexts;
using Uniject.Reflection;
using UnityEngine;

namespace Uniject.InstanceGetters
{
    public class InstanceGetterFromComponentInHierarchy : InstanceGetter
    {
        public InstanceGetterFromComponentInHierarchy(Container container, Type concreteType) : base(container)
        {
            if (!TypeValidator.TypeIsInterfaceOrComponent(concreteType))
                throw new ArgumentException(
                    $"Type {concreteType} for {nameof(InstanceGetterFromComponentInHierarchy)} " +
                    "must be a Component or an interface.");
        }

        public override object GetInstance(
            Type concreteType,
            CreateOptions createOptions,
            InjectContext context)
        {
            var visitedTransforms = new HashSet<Transform>();
            var checkedContexts = new List<string>();
            var hasContext = false;

            foreach (var currentContainer in context.Container.GetSelfAndParents())
            {
                var currentContext = currentContainer.Context;

                if (ReferenceEquals(currentContext, null))
                    continue;

                if (currentContext == null)
                    throw new InvalidOperationException(
                        $"{nameof(InstanceGetterFromComponentInHierarchy)} encountered a destroyed Context " +
                        "in the binding owner's container hierarchy.");

                hasContext = true;
                checkedContexts.Add(
                    $"{currentContext.GetType().Name} on GameObject '{currentContext.gameObject.name}'");

                var component = FindInContext(
                    currentContext,
                    currentContainer,
                    concreteType,
                    visitedTransforms);

                if (component != null)
                    return component;
            }

            if (!hasContext)
                throw new InvalidOperationException(
                    $"{nameof(InstanceGetterFromComponentInHierarchy)} requires a live GameObjectContext " +
                    "or SceneContext in the binding owner's container hierarchy.");

            throw new InvalidOperationException(
                $"{nameof(InstanceGetterFromComponentInHierarchy)} could not find a component assignable to type " +
                $"{concreteType}. Checked contexts: {string.Join(" -> ", checkedContexts)}.");
        }

        private static Component FindInContext(
            Context context,
            Container container,
            Type concreteType,
            HashSet<Transform> visitedTransforms)
        {
            if (context is GameObjectContext gameObjectContext)
            {
                var component = FindFromRoot(
                    gameObjectContext.transform,
                    concreteType,
                    container,
                    visitedTransforms);

                return component != null
                    ? component
                    : FindFromRoot(
                        container.ParentTransformForGameObjects,
                        concreteType,
                        container,
                        visitedTransforms);
            }

            if (context is SceneContext sceneContext)
            {
                var scene = sceneContext.gameObject.scene;

                if (!scene.IsValid() || !scene.isLoaded)
                    throw new InvalidOperationException(
                        $"{nameof(InstanceGetterFromComponentInHierarchy)} cannot search " +
                        $"{sceneContext.GetType().Name} because its Scene is invalid or not loaded.");

                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    var component = FindFromRoot(
                        rootGameObject.transform,
                        concreteType,
                        container,
                        visitedTransforms);

                    if (component != null)
                        return component;
                }

                return FindFromRoot(
                    container.ParentTransformForGameObjects,
                    concreteType,
                    container,
                    visitedTransforms);
            }

            throw new InvalidOperationException(
                $"{nameof(InstanceGetterFromComponentInHierarchy)} does not support Context type " +
                $"{context.GetType()}. Only GameObjectContext and SceneContext are supported.");
        }

        private static Component FindFromRoot(Transform root, Type concreteType, Container contextContainer, HashSet<Transform> visitedTransforms)
        {
            if (root == null || visitedTransforms.Contains(root))
                return null;

            var pool = StaticCollections.collectionPool;
            var contexts = pool.SpawnList<Context>();
            Stack<Transform> pendingTransforms = null;

            try
            {
                if (IsInsideLogicalDescendantContext(root, contextContainer, contexts))
                    return null;

                if (visitedTransforms.Count == 0)
                {
                    // Частый случай: компонент находится прямо на корне.
                    if (root.TryGetComponent(concreteType, out var rootComponent))
                        return rootComponent;

                    // Проверяем структуру одним native-обходом.
                    root.GetComponentsInChildren<Context>(true, contexts);

                    var hasNestedContext = false;

                    foreach (var foundContext in contexts)
                    {
                        if (foundContext != null && foundContext.transform != root)
                        {
                            hasNestedContext = true;
                            break;
                        }
                    }

                    if (!hasNestedContext)
                    {
                        var candidate = root.GetComponentInChildren(concreteType, true);

                        // Сохраняем выбор компонента на найденном GameObject
                        // через тот же локальный API, который использует текущий DFS.
                        if (candidate != null &&
                            candidate.gameObject.TryGetComponent(
                                concreteType, out var resolvedComponent))
                        {
                            return resolvedComponent;
                        }
                    }
                }

                // Далее существующий DFS.

                pendingTransforms = pool.SpawnStack<Transform>();
                pendingTransforms.Push(root);

                while (pendingTransforms.Count > 0)
                {
                    var currentTransform = pendingTransforms.Pop();

                    if (currentTransform == null || !visitedTransforms.Add(currentTransform))
                        continue;

                    var gameObject = currentTransform.gameObject;

                    if (HasLogicalDescendantContext(gameObject, contextContainer, contexts))
                        continue;

                    if (gameObject.TryGetComponent(concreteType, out var component))
                        return component;

                    for (var i = currentTransform.childCount - 1; i >= 0; i--)
                        pendingTransforms.Push(currentTransform.GetChild(i));
                }

                return null;
            }
            finally
            {
                if (pendingTransforms != null)
                    pool.DespawnStack(pendingTransforms);

                pool.DespawnList(contexts);
            }
        }

        private static bool IsInsideLogicalDescendantContext(Transform transform, Container contextContainer, List<Context> contexts)
        {
            var currentTransform = transform;

            while (currentTransform != null)
            {
                if (HasLogicalDescendantContext(
                        currentTransform.gameObject,
                        contextContainer,
                        contexts))
                {
                    return true;
                }

                currentTransform = currentTransform.parent;
            }

            return false;
        }

        private static bool HasLogicalDescendantContext(GameObject gameObject, Container contextContainer, List<Context> contexts)
        {
            gameObject.GetComponents(contexts);

            foreach (var context in contexts)
            {
                if (context == null || context.Container == null)
                    continue;

                if (context.Container.IsStrictDescendantOf(contextContainer))
                    return true;
            }

            return false;
        }
    }
}
