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

        private static Component FindFromRoot(
            Transform root,
            Type concreteType,
            Container contextContainer,
            HashSet<Transform> visitedTransforms)
        {
            if (root == null ||
                visitedTransforms.Contains(root) ||
                IsInsideLogicalDescendantContext(root, contextContainer))
                return null;

            var pendingTransforms = new Stack<Transform>();
            pendingTransforms.Push(root);

            while (pendingTransforms.Count > 0)
            {
                var currentTransform = pendingTransforms.Pop();

                if (currentTransform == null || !visitedTransforms.Add(currentTransform))
                    continue;

                if (HasLogicalDescendantContext(currentTransform.gameObject, contextContainer))
                    continue;

                var component = currentTransform.gameObject.GetComponent(concreteType);

                if (component != null)
                    return component;

                for (var i = currentTransform.childCount - 1; i >= 0; i--)
                    pendingTransforms.Push(currentTransform.GetChild(i));
            }

            return null;
        }

        private static bool IsInsideLogicalDescendantContext(
            Transform transform,
            Container contextContainer)
        {
            var currentTransform = transform;

            while (currentTransform != null)
            {
                if (HasLogicalDescendantContext(currentTransform.gameObject, contextContainer))
                    return true;

                currentTransform = currentTransform.parent;
            }

            return false;
        }

        private static bool HasLogicalDescendantContext(
            GameObject gameObject,
            Container contextContainer)
        {
            foreach (var context in gameObject.GetComponents<Context>())
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
