using Uniject.Contexts;
using UnityEditor;
using UnityEngine;

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

            var disableInstallers = IsEnabledForEverySelectedObject(_useSiblingInstallers);
            var disableInjectTargets =
                IsEnabledForEverySelectedObject(_injectInAllContextGameObjects);

            var property = serializedObject.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.propertyPath == "_installers")
                {
                    DrawCollectionFoldout(_useSiblingInstallers, _installers, disableInstallers);
                    continue;
                }

                if (property.propertyPath == "_injectTargets")
                {
                    DrawCollectionFoldout(
                        _injectInAllContextGameObjects,
                        _injectTargets,
                        disableInjectTargets);
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

        private static void DrawCollectionFoldout(
            SerializedProperty toggleProperty,
            SerializedProperty collectionProperty,
            bool disableCollection)
        {
            var headerRect = EditorGUILayout.GetControlRect();
            var toggleWidth = EditorGUIUtility.singleLineHeight;
            var toggleRect = new Rect(
                headerRect.xMax - toggleWidth,
                headerRect.y,
                toggleWidth,
                headerRect.height);
            var foldoutRect = headerRect;
            foldoutRect.xMax = toggleRect.xMin - EditorGUIUtility.standardVerticalSpacing;

            toggleProperty.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                toggleProperty.isExpanded,
                toggleProperty.displayName,
                true);
            EditorGUI.PropertyField(toggleRect, toggleProperty, GUIContent.none);

            if (!toggleProperty.isExpanded)
                return;

            using (new EditorGUI.DisabledScope(disableCollection))
            {
                EditorGUI.indentLevel++;

                try
                {
                    EditorGUILayout.PropertyField(collectionProperty.FindPropertyRelative("Array.size"));

                    for (var index = 0; index < collectionProperty.arraySize; index++)
                        EditorGUILayout.PropertyField(collectionProperty.GetArrayElementAtIndex(index), true);
                }
                finally
                {
                    EditorGUI.indentLevel--;
                }
            }
        }

        private static bool IsEnabledForEverySelectedObject(SerializedProperty property)
        {
            return !property.hasMultipleDifferentValues && property.boolValue;
        }
    }
}
