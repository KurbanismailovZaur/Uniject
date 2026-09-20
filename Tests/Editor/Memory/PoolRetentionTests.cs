using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Uniject.Tests.Memory
{
    [Category("MemoryRetention")]
    public class PoolRetentionTests
    {
        public enum PoolState { Spawned, Despawned, Mixed }

        public sealed class Product
        {
            public object Payload = new object();
            public Product() { }
        }

        public sealed class ProductPool : Pool<Product> { }
        public sealed class ResettingProductPool : Pool<Product>
        {
            protected override void Reset(Product instance) => instance.Payload = null;
        }

        [Test]
        public void Clear_ReleasesInstancesAndTheirPayloads([Values] PoolState state)
        {
            using var container = new Container();
            var pool = CreatePool<ProductPool>(container);
            var references = Populate(pool, state);
            MemoryRetentionAssert.Retained(pool, references);

            pool.Clear();

            Assert.That(pool.InstanceCount, Is.Zero);
            MemoryRetentionAssert.Collected(pool, references);
        }

        [Test]
        public void Dispose_ReleasesInstancesAndTheirPayloads([Values] PoolState state)
        {
            using var container = new Container();
            var pool = CreatePool<ProductPool>(container);
            var references = Populate(pool, state);

            pool.Dispose();

            Assert.That(pool.InstanceCount, Is.Zero);
            MemoryRetentionAssert.Collected(pool, references);
        }

        [Test]
        public void Clear_ReleasesAdoptedInstances()
        {
            using var container = new Container();
            var pool = CreatePool<ProductPool>(container);
            var reference = Adopt(pool);
            MemoryRetentionAssert.Retained(pool, reference);

            pool.Clear();

            MemoryRetentionAssert.Collected(pool, reference);
        }

        [Test]
        public void Despawn_ResetReleasesPayloadWhileInstanceRemainsReusable()
        {
            using var container = new Container();
            var pool = CreatePool<ResettingProductPool>(container);
            var references = SpawnWithPayloadAndDespawn(pool);

            MemoryRetentionAssert.Collected(pool, references[1]);
            MemoryRetentionAssert.Retained(pool, references[0]);
            Assert.That(pool.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void RepeatedPopulateAndClear_DoesNotRetainPreviousGenerations()
        {
            using var container = new Container();
            var pool = CreatePool<ProductPool>(container);
            var references = new WeakReference[16 * 8];
            for (var i = 0; i < 16; i++)
            {
                Array.Copy(Populate(pool, PoolState.Mixed), 0, references, i * 8, 8);
                pool.Clear();
            }

            MemoryRetentionAssert.Collected(pool, references);
        }

        private static TPool CreatePool<TPool>(Container container) where TPool : Pool<Product>, new()
        {
            container.BindPool<Product, TPool>().FromConstructor().AsCached();
            return container.Resolve<TPool>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] Populate(ProductPool pool, PoolState state)
        {
            var references = new WeakReference[8];
            var products = new Product[4];
            for (var i = 0; i < products.Length; i++)
            {
                var product = pool.Spawn();
                products[i] = product;
                references[i * 2] = new WeakReference(product);
                references[i * 2 + 1] = new WeakReference(product.Payload);
            }

            for (var i = 0; i < products.Length; i++)
            {
                if (state == PoolState.Despawned || state == PoolState.Mixed && i % 2 == 0)
                    pool.Despawn(products[i]);
            }

            return references;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference Adopt(ProductPool pool)
        {
            var product = new Product();
            pool.Adopt(product);
            return new WeakReference(product);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] SpawnWithPayloadAndDespawn(ResettingProductPool pool)
        {
            var product = pool.Spawn();
            product.Payload = new object();
            var references = new[] { new WeakReference(product), new WeakReference(product.Payload) };
            pool.Despawn(product);
            return references;
        }
    }
}
