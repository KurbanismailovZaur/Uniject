using Uniject.Contexts;
using UnityEditor;

namespace Uniject.Editor
{
    [CustomEditor(typeof(Context), true)]
    [CanEditMultipleObjects]
    public sealed class ContextEditor : UnityEditor.Editor
    {
        private SerializedProperty _installers;
        private SerializedProperty _injectTargets;
        private SerializedProperty _useSiblingInstallers;
        private SerializedProperty _injectInAllContextGameObjects;

        private void OnEnable()
        {
            _installers = serializedObject.FindProperty("_installers");
            _injectTargets = serializedObject.FindProperty("_injectTargets");
            _useSiblingInstallers = serializedObject.FindProperty("_useSiblingInstallers");
            _injectInAllContextGameObjects =
                serializedObject.FindProperty("_injectInAllContextGameObjects");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var hideInstallers = IsEnabledForEverySelectedObject(_useSiblingInstallers);
            var hideInjectTargets =
                IsEnabledForEverySelectedObject(_injectInAllContextGameObjects);

            var property = serializedObject.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "_installers")
                {
                    DrawInstallersRow(hideInstallers);
                    continue;
                }

                if (property.propertyPath == "_injectTargets")
                {
                    DrawInjectTargetsRow(hideInjectTargets);
                    continue;
                }

                if (property.propertyPath == "_useSiblingInstallers")
                    continue;

                if (property.propertyPath == "_injectInAllContextGameObjects")
                    continue;

                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                    EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInstallersRow(bool hideInstallers)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_useSiblingInstallers);

                if (!hideInstallers)
                    EditorGUILayout.PropertyField(_installers, true);
            }
        }

        private void DrawInjectTargetsRow(bool hideInjectTargets)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_injectInAllContextGameObjects);

                if (!hideInjectTargets)
                    EditorGUILayout.PropertyField(_injectTargets, true);
            }
        }

        private static bool IsEnabledForEverySelectedObject(SerializedProperty property)
        {
            return !property.hasMultipleDifferentValues && property.boolValue;
        }
    }
}
