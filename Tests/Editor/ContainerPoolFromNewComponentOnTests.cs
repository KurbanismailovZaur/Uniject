using NUnit.Framework;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public class ContainerPoolFromNewComponentOnTests : ContainerPoolTestFixture
    {
        private class InterfacePool : Pool<IInterface> { }

        [Test]
        public void SpawnAndDespawn_FromNewComponentOn_UsesDefaultGameObjectActivation()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.BindPool<Script, PooledScriptPool>()
                    .WithInitialSize(1)
                    .FromNewComponentOn(gameObject)
                    .AsCached();

                var pool = container.Resolve<PooledScriptPool>();
                var component = gameObject.GetComponent<Script>();

                Assert.That(pool.WithoutGameObjectActivation, Is.False);
                Assert.That(component, Is.Not.Null);
                Assert.That(gameObject.activeSelf, Is.False);

                var spawned = pool.Spawn();

                Assert.That(spawned, Is.SameAs(component));
                Assert.That(gameObject.activeSelf, Is.True);

                pool.Despawn(spawned);

                Assert.That(gameObject.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Spawn_FromNewComponentOn_WithManualGameObjectActivationSetting_AddsComponentsWithoutChangingActiveState()
        {
            var gameObject = new GameObject("Target");

            try
            {
                var container = new Container();
                container.BindPool<IInterface, InterfacePool>()
                    .WithInitialSize(2)
                    .WithMaxSize(3)
                    .ExpandByOne()
                    .WithoutGameObjectActivation()
                    .To<ScriptImplementedIInterface>()
                    .FromNewComponentOn(gameObject)
                    .AsCached();

                var pool = container.Resolve<InterfacePool>();

                Assert.That(pool.WithoutGameObjectActivation, Is.True);
                Assert.That(gameObject.activeSelf, Is.True);
                Assert.That(gameObject.GetComponents<ScriptImplementedIInterface>(), Has.Length.EqualTo(2));

                var first = pool.Spawn();
                var second = pool.Spawn();
                var third = pool.Spawn();

                Assert.That(first, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(second, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(third, Is.TypeOf<ScriptImplementedIInterface>());
                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(third, Is.Not.SameAs(first));
                Assert.That(third, Is.Not.SameAs(second));
                Assert.That(((Component)first).gameObject, Is.SameAs(gameObject));
                Assert.That(((Component)second).gameObject, Is.SameAs(gameObject));
                Assert.That(((Component)third).gameObject, Is.SameAs(gameObject));
                Assert.That(gameObject.GetComponents<ScriptImplementedIInterface>(), Has.Length.EqualTo(3));
                Assert.That(gameObject.activeSelf, Is.True);

                pool.Despawn(first);

                Assert.That(gameObject.activeSelf, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
