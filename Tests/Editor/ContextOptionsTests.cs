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
