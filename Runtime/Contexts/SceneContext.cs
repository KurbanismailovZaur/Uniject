using UnityEngine;

namespace Uniject.Contexts
{
    public class SceneContext : Context
    {
        protected void Start() => Run();

        protected override void InjectInAllContextGameObjects()
        {
            var rootGameObjects = StaticCollections.collectionPool.SpawnList<GameObject>();

            try
            {
                gameObject.scene.GetRootGameObjects(rootGameObjects);
                InjectMonoBehavioursInHierarchies(rootGameObjects);
            }
            finally
            {
                StaticCollections.collectionPool.DespawnList(rootGameObjects);
            }
        }
    }
}
