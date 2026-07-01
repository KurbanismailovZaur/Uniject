using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromNewComponentOnNewPrefabTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromNewComponentOnNewPrefab_WhenPrefabIsGameObject_AddsComponentToClonedPrefab()
        {
            var prefab = new GameObject("Prefab");
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, ScriptFactory>().FromNewComponentOnNewPrefab(prefab).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefab));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
                Assert.That(prefab.GetComponent<Script>(), Is.Null);
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [Test]
        public void Create_FromNewComponentOnNewPrefab_WhenPrefabIsComponent_AddsComponentToClonedPrefab()
        {
            var prefabComponent = new GameObject("Prefab").transform;
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, ScriptFactory>().FromNewComponentOnNewPrefab(prefabComponent).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result.gameObject, Is.Not.SameAs(prefabComponent.gameObject));
                Assert.That(result.gameObject.GetComponent<Script>(), Is.SameAs(result));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabComponent.gameObject);
            }
        }
    }
}
