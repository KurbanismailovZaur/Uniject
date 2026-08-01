using UnityEngine;

namespace Uniject.Contexts
{
    public class GameObjectContext : Context
    {
        protected override void InjectInAllContextGameObjects()
        {
            var rootGameObjects = StaticCollections.collectionPool.SpawnList<GameObject>();

            try
            {
                rootGameObjects.Add(gameObject);
                InjectMonoBehavioursInHierarchies(rootGameObjects, transform);
            }
            finally
            {
                StaticCollections.collectionPool.DespawnList(rootGameObjects);
            }
        }
    }
}
