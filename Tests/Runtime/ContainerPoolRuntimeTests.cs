using System.Collections;
using NUnit.Framework;
using Uniject.Tests.Fixtures;
using UnityEngine;
using UnityEngine.TestTools;

namespace Uniject.Tests
{
    public class ContainerPoolRuntimeTests
    {
        private class ScriptPool : Pool<Script> { }

        [UnityTest]
        public IEnumerator Clear_WhenPoolContainsSpawnedAndDespawnedComponents_DestroysTheirGameObjects()
        {
            GameObject spawnedGameObject = null;
            GameObject despawnedGameObject = null;

            try
            {
                var container = new Container();
                container.BindPool<Script, ScriptPool>()
                    .WithInitialSize(2)
                    .ExpandByOne()
                    .FromNewComponentOnNewGameObject()
                    .AsCached();
                var pool = container.Resolve<ScriptPool>();
                var spawned = pool.Spawn();
                var despawned = pool.Spawn();
                spawnedGameObject = spawned.gameObject;
                despawnedGameObject = despawned.gameObject;
                pool.Despawn(despawned);

                pool.Clear();

                Assert.That(pool.InstanceCount, Is.Zero);

                yield return null;

                Assert.That(spawnedGameObject == null, Is.True);
                Assert.That(despawnedGameObject == null, Is.True);
            }
            finally
            {
                if (spawnedGameObject != null)
                    Object.DestroyImmediate(spawnedGameObject);

                if (despawnedGameObject != null)
                    Object.DestroyImmediate(despawnedGameObject);
            }
        }
    }
}
