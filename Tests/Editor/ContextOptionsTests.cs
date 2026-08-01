using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Components;
using Uniject.Contexts;
using Uniject.Installers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests
{
    public sealed class ContextOptionsTestInstaller : MonoInstaller
    {
        public string EventName { get; set; }
        public IList<string> Events { get; set; }
        public Action<Container> InstallAction { get; set; }
        public int InstallCallsCount { get; private set; }

        public override void Install(Container container)
        {
            InstallCallsCount++;
            Events?.Add(EventName);
            InstallAction?.Invoke(container);
        }
    }

    public sealed class ContextOptionsTestDependency
    {
    }

    public sealed class ContextOptionsTestInjectTarget : MonoBehaviour
    {
        public ContextOptionsTestDependency Dependency { get; private set; }
        public int InjectCallsCount { get; private set; }

        [Inject]
        public void Construct(ContextOptionsTestDependency dependency)
        {
            Dependency = dependency;
            InjectCallsCount++;
        }
    }

    public class ContextOptionsTests
    {
        [Test]
        public void Context_WhenCreated_HasAutomaticOptionsEnabledByDefault()
        {
            var contextObject = new GameObject("Context");

            try
            {
                var context = contextObject.AddComponent<SceneContext>();
                var serializedContext = new SerializedObject(context);

                Assert.That(serializedContext.FindProperty("_useSiblingInstallers").boolValue, Is.True);
                Assert.That(
                    serializedContext.FindProperty("_injectInAllContextGameObjects").boolValue,
                    Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void Install_WhenUseSiblingInstallersIsEnabled_UsesOnlySiblingInstallersInComponentOrder()
        {
            var contextObject = new GameObject("Context");
            var configuredInstallerObject = new GameObject("ConfiguredInstaller");

            try
            {
                var events = new List<string>();
                var context = contextObject.AddComponent<SceneContext>();
                var firstSibling = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var secondSibling = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var configured = configuredInstallerObject.AddComponent<ContextOptionsTestInstaller>();
                ConfigureInstaller(firstSibling, "first sibling", events);
                ConfigureInstaller(secondSibling, "second sibling", events);
                ConfigureInstaller(configured, "configured", events);
                ContextTestUtility.Configure(
                    context,
                    installers: new[] { configured },
                    useSiblingInstallers: true);

                context.Initialize();
                context.Install();

                Assert.That(events, Is.EqualTo(new[] { "first sibling", "second sibling" }));
                Assert.That(firstSibling.InstallCallsCount, Is.EqualTo(1));
                Assert.That(secondSibling.InstallCallsCount, Is.EqualTo(1));
                Assert.That(configured.InstallCallsCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(contextObject, configuredInstallerObject);
            }
        }

        [Test]
        public void Install_WhenUseSiblingInstallersIsDisabled_UsesOnlyConfiguredInstallersInListOrder()
        {
            var contextObject = new GameObject("Context");
            var configuredInstallerObject = new GameObject("ConfiguredInstallers");

            try
            {
                var events = new List<string>();
                var context = contextObject.AddComponent<SceneContext>();
                var sibling = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var firstConfigured = configuredInstallerObject.AddComponent<ContextOptionsTestInstaller>();
                var secondConfigured = configuredInstallerObject.AddComponent<ContextOptionsTestInstaller>();
                ConfigureInstaller(sibling, "sibling", events);
                ConfigureInstaller(firstConfigured, "first configured", events);
                ConfigureInstaller(secondConfigured, "second configured", events);
                ContextTestUtility.Configure(
                    context,
                    installers: new[] { secondConfigured, firstConfigured });

                context.Initialize();
                context.Install();

                Assert.That(events, Is.EqualTo(new[] { "second configured", "first configured" }));
                Assert.That(sibling.InstallCallsCount, Is.Zero);
                Assert.That(firstConfigured.InstallCallsCount, Is.EqualTo(1));
                Assert.That(secondConfigured.InstallCallsCount, Is.EqualTo(1));
            }
            finally
            {
                DestroyGameObjects(contextObject, configuredInstallerObject);
            }
        }

        [Test]
        public void Run_WhenInjectInAllContextGameObjectsIsEnabled_InjectsSameSceneRootsAndInactiveObjectsOnce()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var otherScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("Context");
            var activeTargetObject = new GameObject("ActiveTarget");
            var inactiveRoot = new GameObject("InactiveRoot");
            var inactiveTargetObject = new GameObject("InactiveTarget");
            var disabledTargetObject = new GameObject("DisabledTarget");
            var otherSceneTargetObject = new GameObject("OtherSceneTarget");

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);
                SceneManager.MoveGameObjectToScene(activeTargetObject, contextScene);
                SceneManager.MoveGameObjectToScene(inactiveRoot, contextScene);
                SceneManager.MoveGameObjectToScene(inactiveTargetObject, contextScene);
                SceneManager.MoveGameObjectToScene(disabledTargetObject, contextScene);
                SceneManager.MoveGameObjectToScene(otherSceneTargetObject, otherScene);
                inactiveTargetObject.transform.SetParent(inactiveRoot.transform);
                inactiveRoot.SetActive(false);

                var dependency = new ContextOptionsTestDependency();
                var context = contextObject.AddComponent<SceneContext>();
                var installer = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var activeTarget = activeTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var inactiveTarget = inactiveTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var disabledTarget = disabledTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var otherSceneTarget = otherSceneTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                disabledTarget.enabled = false;
                installer.InstallAction = container => container.BindInstance(dependency);
                ContextTestUtility.Configure(
                    context,
                    injectTargets: new[] { otherSceneTarget },
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);

                context.Run();
                context.Run();

                AssertInjectedOnce(activeTarget, dependency);
                AssertInjectedOnce(inactiveTarget, dependency);
                AssertInjectedOnce(disabledTarget, dependency);
                Assert.That(otherSceneTarget.Dependency, Is.Null);
                Assert.That(otherSceneTarget.InjectCallsCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(
                    contextObject,
                    activeTargetObject,
                    inactiveRoot,
                    disabledTargetObject,
                    otherSceneTargetObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);

                if (otherScene.IsValid() && otherScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(otherScene);
            }
        }

        [Test]
        public void Run_WhenSceneAutoInjectionFindsGameObjectContext_PrunesItsGameObjectAndHierarchy()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var contextObject = new GameObject("SceneContext");
            var sceneTargetObject = new GameObject("SceneTarget");
            var nestedContextObject = new GameObject("NestedGameObjectContext");
            var nestedTargetObject = new GameObject("NestedTarget");

            try
            {
                SceneManager.MoveGameObjectToScene(contextObject, contextScene);
                SceneManager.MoveGameObjectToScene(sceneTargetObject, contextScene);
                SceneManager.MoveGameObjectToScene(nestedContextObject, contextScene);
                SceneManager.MoveGameObjectToScene(nestedTargetObject, contextScene);
                nestedContextObject.transform.SetParent(contextObject.transform);
                nestedTargetObject.transform.SetParent(nestedContextObject.transform);

                var dependency = new ContextOptionsTestDependency();
                var context = contextObject.AddComponent<SceneContext>();
                var installer = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var sceneTarget = sceneTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var nestedContext = nestedContextObject.AddComponent<GameObjectContext>();
                var nestedContextTarget = nestedContextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var nestedTarget = nestedTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                installer.InstallAction = container => container.BindInstance(dependency);
                ContextTestUtility.Configure(
                    context,
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);

                context.Run();

                AssertInjectedOnce(sceneTarget, dependency);
                AssertNotInjected(nestedContextTarget);
                AssertNotInjected(nestedTarget);
                Assert.That(nestedContext.IsInitialized, Is.False);
            }
            finally
            {
                DestroyGameObjects(
                    contextObject,
                    sceneTargetObject,
                    nestedContextObject,
                    nestedTargetObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void Run_WhenGameObjectContextUsesAutoInjection_InjectsOnlyItsRootAndOrdinaryDescendants()
        {
            var parentObject = new GameObject("Parent");
            var contextObject = new GameObject("GameObjectContext");
            var childObject = new GameObject("Child");
            var inactiveObject = new GameObject("InactiveChild");
            var siblingObject = new GameObject("Sibling");

            try
            {
                contextObject.transform.SetParent(parentObject.transform);
                childObject.transform.SetParent(contextObject.transform);
                inactiveObject.transform.SetParent(childObject.transform);
                siblingObject.transform.SetParent(parentObject.transform);
                inactiveObject.SetActive(false);

                var dependency = new ContextOptionsTestDependency();
                var context = contextObject.AddComponent<GameObjectContext>();
                var installer = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var rootTarget = contextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var childTarget = childObject.AddComponent<ContextOptionsTestInjectTarget>();
                var inactiveTarget = inactiveObject.AddComponent<ContextOptionsTestInjectTarget>();
                var parentTarget = parentObject.AddComponent<ContextOptionsTestInjectTarget>();
                var siblingTarget = siblingObject.AddComponent<ContextOptionsTestInjectTarget>();
                var injectTargets = contextObject.AddComponent<InjectTargets>();
                SetInjectTargets(injectTargets, siblingTarget);
                installer.InstallAction = container => container.BindInstance(dependency);
                ContextTestUtility.Configure(
                    context,
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);

                context.Run();

                AssertInjectedOnce(rootTarget, dependency);
                AssertInjectedOnce(childTarget, dependency);
                AssertInjectedOnce(inactiveTarget, dependency);
                AssertNotInjected(parentTarget);
                AssertNotInjected(siblingTarget);
            }
            finally
            {
                DestroyGameObjects(parentObject, contextObject, childObject, inactiveObject, siblingObject);
            }
        }

        [Test]
        public void Run_WhenGameObjectContextFindsNestedContexts_PrunesDirectAndDeepContextHierarchies()
        {
            var contextObject = new GameObject("GameObjectContext");
            var directContextObject = new GameObject("DirectContext");
            var directTargetObject = new GameObject("DirectContextTarget");
            var ordinaryBranchObject = new GameObject("OrdinaryBranch");
            var deepContextObject = new GameObject("DeepContext");
            var deepTargetObject = new GameObject("DeepContextTarget");

            try
            {
                directContextObject.transform.SetParent(contextObject.transform);
                directTargetObject.transform.SetParent(directContextObject.transform);
                ordinaryBranchObject.transform.SetParent(contextObject.transform);
                deepContextObject.transform.SetParent(ordinaryBranchObject.transform);
                deepTargetObject.transform.SetParent(deepContextObject.transform);

                var dependency = new ContextOptionsTestDependency();
                var context = contextObject.AddComponent<GameObjectContext>();
                var installer = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var rootTarget = contextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var directContext = directContextObject.AddComponent<GameObjectContext>();
                var directContextTarget = directContextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var directTarget = directTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var ordinaryTarget = ordinaryBranchObject.AddComponent<ContextOptionsTestInjectTarget>();
                var deepContext = deepContextObject.AddComponent<GameObjectContext>();
                var deepContextTarget = deepContextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var deepTarget = deepTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                installer.InstallAction = container => container.BindInstance(dependency);
                ContextTestUtility.Configure(
                    context,
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);

                context.Run();

                AssertInjectedOnce(rootTarget, dependency);
                AssertInjectedOnce(ordinaryTarget, dependency);
                AssertNotInjected(directContextTarget);
                AssertNotInjected(directTarget);
                AssertNotInjected(deepContextTarget);
                AssertNotInjected(deepTarget);
                Assert.That(directContext.IsInitialized, Is.False);
                Assert.That(deepContext.IsInitialized, Is.False);
            }
            finally
            {
                DestroyGameObjects(
                    contextObject,
                    directContextObject,
                    directTargetObject,
                    ordinaryBranchObject,
                    deepContextObject,
                    deepTargetObject);
            }
        }

        [Test]
        public void Run_WithNestedAutoInjectionContexts_InjectsEachTargetOnceUsingItsOwnerContainer()
        {
            var contextScene = EditorSceneManager.NewPreviewScene();
            var sceneContextObject = new GameObject("SceneContext");
            var sceneTargetObject = new GameObject("SceneTarget");
            var childContextObject = new GameObject("ChildContext");
            var childTargetObject = new GameObject("ChildTarget");
            var grandchildContextObject = new GameObject("GrandchildContext");
            var grandchildTargetObject = new GameObject("GrandchildTarget");

            try
            {
                sceneTargetObject.transform.SetParent(sceneContextObject.transform);
                childContextObject.transform.SetParent(sceneContextObject.transform);
                childTargetObject.transform.SetParent(childContextObject.transform);
                grandchildContextObject.transform.SetParent(childContextObject.transform);
                grandchildTargetObject.transform.SetParent(grandchildContextObject.transform);
                SceneManager.MoveGameObjectToScene(sceneContextObject, contextScene);

                var sceneDependency = new ContextOptionsTestDependency();
                var childDependency = new ContextOptionsTestDependency();
                var grandchildDependency = new ContextOptionsTestDependency();
                var sceneContext = sceneContextObject.AddComponent<SceneContext>();
                var childContext = childContextObject.AddComponent<GameObjectContext>();
                var grandchildContext = grandchildContextObject.AddComponent<GameObjectContext>();
                var sceneInstaller = sceneContextObject.AddComponent<ContextOptionsTestInstaller>();
                var childInstaller = childContextObject.AddComponent<ContextOptionsTestInstaller>();
                var grandchildInstaller = grandchildContextObject.AddComponent<ContextOptionsTestInstaller>();
                var sceneTarget = sceneTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var childContextTarget = childContextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var childTarget = childTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var grandchildContextTarget = grandchildContextObject.AddComponent<ContextOptionsTestInjectTarget>();
                var grandchildTarget = grandchildTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                sceneInstaller.InstallAction = container => container.BindInstance(sceneDependency);
                childInstaller.InstallAction = container => container.BindInstance(childDependency);
                grandchildInstaller.InstallAction = container => container.BindInstance(grandchildDependency);
                ContextTestUtility.Configure(
                    grandchildContext,
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);
                ContextTestUtility.Configure(
                    childContext,
                    gameObjectContexts: new[] { grandchildContext },
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);
                ContextTestUtility.Configure(
                    sceneContext,
                    gameObjectContexts: new[] { childContext },
                    useSiblingInstallers: true,
                    injectInAllContextGameObjects: true);

                sceneContext.Run();

                AssertInjectedOnce(sceneTarget, sceneDependency);
                AssertInjectedOnce(childContextTarget, childDependency);
                AssertInjectedOnce(childTarget, childDependency);
                AssertInjectedOnce(grandchildContextTarget, grandchildDependency);
                AssertInjectedOnce(grandchildTarget, grandchildDependency);
            }
            finally
            {
                DestroyGameObjects(
                    sceneContextObject,
                    sceneTargetObject,
                    childContextObject,
                    childTargetObject,
                    grandchildContextObject,
                    grandchildTargetObject);

                if (contextScene.IsValid() && contextScene.isLoaded)
                    EditorSceneManager.ClosePreviewScene(contextScene);
            }
        }

        [Test]
        public void Run_WhenInjectInAllContextGameObjectsIsDisabled_InjectsOnlyConfiguredTargets()
        {
            var contextObject = new GameObject("Context");
            var directTargetObject = new GameObject("DirectTarget");
            var wrappedTargetObject = new GameObject("WrappedTarget");
            var wrapperObject = new GameObject("InjectTargets");
            var unlistedTargetObject = new GameObject("UnlistedTarget");

            try
            {
                var dependency = new ContextOptionsTestDependency();
                var context = contextObject.AddComponent<SceneContext>();
                var installer = contextObject.AddComponent<ContextOptionsTestInstaller>();
                var directTarget = directTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var wrappedTarget = wrappedTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                var wrapper = wrapperObject.AddComponent<InjectTargets>();
                var unlistedTarget = unlistedTargetObject.AddComponent<ContextOptionsTestInjectTarget>();
                directTargetObject.SetActive(false);
                SetInjectTargets(wrapper, wrappedTarget);
                installer.InstallAction = container => container.BindInstance(dependency);
                ContextTestUtility.Configure(
                    context,
                    injectTargets: new MonoBehaviour[] { directTarget, wrapper },
                    useSiblingInstallers: true);

                context.Run();
                context.Run();

                AssertInjectedOnce(directTarget, dependency);
                AssertInjectedOnce(wrappedTarget, dependency);
                Assert.That(unlistedTarget.Dependency, Is.Null);
                Assert.That(unlistedTarget.InjectCallsCount, Is.Zero);
            }
            finally
            {
                DestroyGameObjects(
                    contextObject,
                    directTargetObject,
                    wrappedTargetObject,
                    wrapperObject,
                    unlistedTargetObject);
            }
        }

        private static void ConfigureInstaller(
            ContextOptionsTestInstaller installer,
            string eventName,
            IList<string> events)
        {
            installer.EventName = eventName;
            installer.Events = events;
        }

        private static void SetInjectTargets(InjectTargets injectTargets, params MonoBehaviour[] targets)
        {
            var targetsField = typeof(InjectTargets).GetField(
                "<Targets>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (targetsField == null)
                throw new MissingFieldException(typeof(InjectTargets).FullName, "<Targets>k__BackingField");

            targetsField.SetValue(injectTargets, targets);
        }

        private static void AssertInjectedOnce(
            ContextOptionsTestInjectTarget target,
            ContextOptionsTestDependency dependency)
        {
            Assert.That(target.Dependency, Is.SameAs(dependency));
            Assert.That(target.InjectCallsCount, Is.EqualTo(1));
        }

        private static void AssertNotInjected(ContextOptionsTestInjectTarget target)
        {
            Assert.That(target.Dependency, Is.Null);
            Assert.That(target.InjectCallsCount, Is.Zero);
        }

        private static void DestroyGameObjects(params GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject != null)
                    UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
