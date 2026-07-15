using System;
using System.Collections.Generic;
using System.Reflection;
using Uniject.Contexts;
using Uniject.Installers;
using UnityEditor;
using UnityEngine;

namespace Uniject.Tests
{
    internal static class ContextTestUtility
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static void Configure(
            Context context,
            IEnumerable<MonoInstaller> installers = null,
            IEnumerable<MonoBehaviour> injectTargets = null,
            IEnumerable<GameObjectContext> gameObjectContexts = null,
            Transform parentTransformForGameObjects = null)
        {
            SetField(context, "_installers", new List<MonoInstaller>(installers ?? Array.Empty<MonoInstaller>()));
            SetField(context, "_injectTargets", new List<MonoBehaviour>(injectTargets ?? Array.Empty<MonoBehaviour>()));
            SetField(context, "_gameObjectContexts", new List<GameObjectContext>(gameObjectContexts ?? Array.Empty<GameObjectContext>()));
            SetField(context, "ParentTransformForGameObjects", parentTransformForGameObjects);
            EditorUtility.SetDirty(context);
        }

        private static void SetField(Context context, string fieldName, object value)
        {
            var field = typeof(Context).GetField(fieldName, FieldFlags);

            if (field == null)
                throw new MissingFieldException(typeof(Context).FullName, fieldName);

            field.SetValue(context, value);
        }
    }
}
