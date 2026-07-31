using System;
using System.Reflection;
using NUnit.Framework;
using Uniject.Contexts;
using UnityEditor;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContextEditorTests
    {
        private const string ContextEditorTypeName = "Uniject.Editor.ContextEditor";

        [TestCase(typeof(SceneContext))]
        [TestCase(typeof(GameObjectContext))]
        public void CreateEditor_ForContextAndDerivedTypes_UsesContextEditor(Type contextType)
        {
            var contextObject = new GameObject("Context");
            UnityEditor.Editor contextEditor = null;

            try
            {
                var context = (Context)contextObject.AddComponent(contextType);

                contextEditor = UnityEditor.Editor.CreateEditor(context);

                Assert.That(contextEditor, Is.Not.Null);
                Assert.That(contextEditor.GetType().FullName, Is.EqualTo(ContextEditorTypeName));
            }
            finally
            {
                if (contextEditor != null)
                    UnityEngine.Object.DestroyImmediate(contextEditor);

                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [Test]
        public void OnEnable_BindsAllContextOptionProperties()
        {
            var contextObject = new GameObject("Context");
            UnityEditor.Editor contextEditor = null;

            try
            {
                var context = contextObject.AddComponent<SceneContext>();
                contextEditor = UnityEditor.Editor.CreateEditor(context);

                AssertBoundProperty(contextEditor, "_installers");
                AssertBoundProperty(contextEditor, "_injectTargets");
                AssertBoundProperty(contextEditor, "_useSiblingInstallers");
                AssertBoundProperty(contextEditor, "_injectInAllContextGameObjects");
            }
            finally
            {
                if (contextEditor != null)
                    UnityEngine.Object.DestroyImmediate(contextEditor);

                UnityEngine.Object.DestroyImmediate(contextObject);
            }
        }

        [TestCase("_useSiblingInstallers")]
        [TestCase("_injectInAllContextGameObjects")]
        public void IsEnabledForEverySelectedObject_HandlesUniformAndMixedValues(string propertyPath)
        {
            var firstObject = new GameObject("FirstContext");
            var secondObject = new GameObject("SecondContext");
            UnityEditor.Editor contextEditor = null;

            try
            {
                var first = firstObject.AddComponent<SceneContext>();
                var second = secondObject.AddComponent<SceneContext>();
                contextEditor = UnityEditor.Editor.CreateEditor(new UnityEngine.Object[] { first, second });
                var decisionMethod = contextEditor.GetType().GetMethod(
                    "IsEnabledForEverySelectedObject",
                    BindingFlags.Static | BindingFlags.NonPublic);

                Assert.That(contextEditor.GetType().FullName, Is.EqualTo(ContextEditorTypeName));
                Assert.That(contextEditor.targets, Has.Length.EqualTo(2));
                Assert.That(decisionMethod, Is.Not.Null);

                SetBooleanProperty(first, propertyPath, true);
                SetBooleanProperty(second, propertyPath, true);
                Assert.That(EvaluateDecision(decisionMethod, first, second, propertyPath), Is.True);

                SetBooleanProperty(second, propertyPath, false);
                Assert.That(EvaluateDecision(decisionMethod, first, second, propertyPath), Is.False);

                SetBooleanProperty(first, propertyPath, false);
                Assert.That(EvaluateDecision(decisionMethod, first, second, propertyPath), Is.False);
            }
            finally
            {
                if (contextEditor != null)
                    UnityEngine.Object.DestroyImmediate(contextEditor);

                UnityEngine.Object.DestroyImmediate(firstObject);
                UnityEngine.Object.DestroyImmediate(secondObject);
            }
        }

        private static void AssertBoundProperty(UnityEditor.Editor contextEditor, string fieldName)
        {
            var field = contextEditor.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var property = field?.GetValue(contextEditor) as SerializedProperty;

            Assert.That(field, Is.Not.Null, $"Editor field {fieldName} was not found.");
            Assert.That(property, Is.Not.Null, $"Serialized property {fieldName} was not bound.");
            Assert.That(property.propertyPath, Is.EqualTo(fieldName));
        }

        private static bool EvaluateDecision(
            MethodInfo decisionMethod,
            Context first,
            Context second,
            string propertyPath)
        {
            var serializedContexts = new SerializedObject(new UnityEngine.Object[] { first, second });
            var property = serializedContexts.FindProperty(propertyPath);

            return (bool)decisionMethod.Invoke(null, new object[] { property });
        }

        private static void SetBooleanProperty(Context context, string propertyPath, bool value)
        {
            var serializedContext = new SerializedObject(context);
            var property = serializedContext.FindProperty(propertyPath);
            property.boolValue = value;
            serializedContext.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
