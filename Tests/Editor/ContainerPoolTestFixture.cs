using System.Collections.Generic;
using Uniject.Bindings.Pools;
using Uniject.Tests.Fixtures;
using UnityEngine;

namespace Uniject.Tests
{
    public abstract class ContainerPoolTestFixture
    {
        protected class Product
        {
            public int Value { get; set; }
        }

        protected class ProductPool : Pool<Product>
        {
            public int ResetCallsCount { get; private set; }

            protected override void Reset(Product instance)
            {
                ResetCallsCount++;
                instance.Value = 0;
            }
        }

        protected class DuplicateProductFactory : CustomFactory<Product>
        {
            private static Product _instance;

            public static void Reset() => _instance = null;

            public override Product Create() => _instance ??= new Product();
        }

        protected class NullProductFactory : CustomFactory<Product>
        {
            public override Product Create() => null;
        }

        protected class EquatableProduct
        {
            public override bool Equals(object obj) => obj is EquatableProduct;

            public override int GetHashCode() => 0;
        }

        protected class EquatableProductPool : Pool<EquatableProduct> { }

        protected class PooledScriptPool : Pool<Script> { }

        protected class PooledScriptFactory : CustomFactory<Script>
        {
            private static readonly List<Script> _createdInstances = new();

            public static IReadOnlyList<Script> CreatedInstances => _createdInstances;

            public override Script Create()
            {
                var instance = new GameObject(nameof(Script)).AddComponent<Script>();
                _createdInstances.Add(instance);
                return instance;
            }

            public static void DestroyCreatedInstances()
            {
                foreach (var instance in _createdInstances)
                {
                    if (instance != null)
                        Object.DestroyImmediate(instance.gameObject);
                }

                _createdInstances.Clear();
            }
        }

        protected static ProductPool CreateProductPool(int initialSize = 0, int maxSize = -1,
            ExpandType expandType = ExpandType.ByOne)
        {
            var container = new Container();
            var expandBuilder = container.BindPool<Product, ProductPool>()
                .WithInitialSize(initialSize)
                .WithMaxSize(maxSize);

            if (expandType == ExpandType.ByOne)
                expandBuilder.ExpandByOne().FromConstructor().AsCached();
            else
                expandBuilder.ExpandByDoubling().FromConstructor().AsCached();

            return container.Resolve<ProductPool>();
        }
    }
}
