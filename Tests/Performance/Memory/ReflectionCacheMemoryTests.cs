using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;
using Unity.PerformanceTesting;
using UnityEngine.Scripting;

namespace Uniject.Tests.Performance.Memory
{
    public class ReflectionCacheMemoryTests
    {
        [Test, Performance]
        public void ConstructorLookup_AfterWarmup()
        {
            ReflectionCache.GetConstructorInjectionData(typeof(ConstructorTarget));
            AllocationMeasurement.Run("ConstructorCacheHit",
                () => ReflectionCache.GetConstructorInjectionData(typeof(ConstructorTarget)), expectZero: true);
        }

        [Test, Performance]
        public void MethodLookup_AfterWarmup([Values(false, true)] bool hasInjectMethod)
        {
            var type = hasInjectMethod ? typeof(MethodTarget) : typeof(ConstructorTarget);
            ReflectionCache.GetMethodInjectionData(type);
            AllocationMeasurement.Run("MethodCacheHit", () => ReflectionCache.GetMethodInjectionData(type),
                expectZero: true);
        }

        [Test, Performance]
        public void ConstructorCacheMiss_WithWarmRuntimeReflection()
        {
            // Evict only this fixture's entry. CLR reflection caches remain warm; this is not a cold domain.
            var cache = GetCache("_constructors");
            var type = typeof(ConstructorTarget);
            var existed = cache.Contains(type);
            var previous = cache[type];
            try
            {
                ReflectionCache.GetConstructorInjectionData(type);
                AllocationMeasurement.Run("ConstructorCacheMiss",
                    () => ReflectionCache.GetConstructorInjectionData(type),
                    setUp: () => cache.Remove(type), iterations: 1);
            }
            finally
            {
                Restore(cache, type, existed, previous);
            }
        }

        [Test, Performance]
        public void MethodCacheMiss_WithWarmRuntimeReflection([Values(false, true)] bool hasInjectMethod)
        {
            var cache = GetCache("_methods");
            var type = hasInjectMethod ? typeof(MethodTarget) : typeof(ConstructorTarget);
            var existed = cache.Contains(type);
            var previous = cache[type];
            try
            {
                ReflectionCache.GetMethodInjectionData(type);
                AllocationMeasurement.Run("MethodCacheMiss",
                    () => ReflectionCache.GetMethodInjectionData(type),
                    setUp: () => cache.Remove(type), iterations: 1);
            }
            finally
            {
                Restore(cache, type, existed, previous);
            }
        }

        [Test, Performance]
        public void RepeatedContainers_WithSameTypes_DoNotAddCacheEntries()
        {
            using (var warmup = new Container())
            {
                warmup.BindInstance(new Dependency());
                warmup.Instantiate<ConstructorTarget>();
                warmup.Inject(new MethodTarget());
            }

            var constructors = GetCache("_constructors");
            var methods = GetCache("_methods");
            var constructorCount = constructors.Count;
            var methodCount = methods.Count;
            AllocationMeasurement.Run("RepeatedContainers", () =>
            {
                using var container = new Container();
                container.BindInstance(new Dependency());
                container.Instantiate<ConstructorTarget>();
                container.Inject(new MethodTarget());
            }, iterations: 10);

            Assert.That(constructors.Count, Is.EqualTo(constructorCount));
            Assert.That(methods.Count, Is.EqualTo(methodCount));
        }

        private static IDictionary GetCache(string name)
        {
            var field = typeof(ReflectionCache).GetField(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "ReflectionCache layout changed; update cache-isolation fixture.");
            return (IDictionary)field.GetValue(null);
        }

        private static void Restore(IDictionary cache, Type type, bool existed, object previous)
        {
            if (existed)
                cache[type] = previous;
            else
                cache.Remove(type);
        }

        [Preserve]
        public class Dependency { }

        [Preserve]
        public class ConstructorTarget
        {
            [Preserve]
            public ConstructorTarget(Dependency dependency) { }
        }

        [Preserve]
        public class MethodTarget
        {
            [Inject, Preserve]
            public void Construct(Dependency dependency) { }
        }
    }
}
