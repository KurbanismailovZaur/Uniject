using System;
using NUnit.Framework;
using Uniject;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerFactoryFromComponentInNewPrefabTests : ContainerFactoryTestFixture
    {
        [Test]
        public void Create_FromComponentInNewPrefab_WhenPrefabIsGameObject_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, ScriptFactory>().FromComponentInNewPrefab(prefabScript.gameObject).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }

        [Test]
        public void Create_FromComponentInNewPrefab_WhenPrefabIsComponent_ReturnsComponentFromClonedPrefab()
        {
            var prefabScript = new GameObject("Prefab").AddComponent<Script>();
            var result = default(Script);

            try
            {
                var container = new Container();
                container.BindFactory<Script, ScriptFactory>().FromComponentInNewPrefab(prefabScript).AsTransient();

                result = container.Resolve<ScriptFactory>().Create();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.SameAs(prefabScript));
                Assert.That(result.gameObject, Is.Not.SameAs(prefabScript.gameObject));
            }
            finally
            {
                if (result != null)
                    UnityEngine.Object.DestroyImmediate(result.gameObject);

                UnityEngine.Object.DestroyImmediate(prefabScript.gameObject);
            }
        }
    }
}
