using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;

namespace Uniject.Tests.Memory
{
    [Category("MemoryRetention")]
    public class ReflectionCacheRetentionTests
    {
        private sealed class Dependency { }

        private sealed class ConstructedTarget
        {
            public readonly Dependency Dependency;
            public ConstructedTarget(Dependency dependency) => Dependency = dependency;
        }

        private sealed class InjectedTarget
        {
            public Dependency Dependency;
            [Inject]
            public void Construct(Dependency dependency) => Dependency = dependency;
        }

        [Test]
        public void CachedConstructorMetadata_DoesNotRetainConstructedTargetsOrArguments()
        {
            var owners = new Container[16];
            var references = CreateInstances(owners, true);
            try
            {
                // Metadata deliberately survives all containers; instance arguments must not.
                var metadata = ReflectionCache.GetConstructorInjectionData(typeof(ConstructedTarget));
                MemoryRetentionAssert.Collected(new object[] { owners, metadata }, references);
            }
            finally
            {
                DisposeAll(owners);
            }
        }

        [Test]
        public void CachedMethodMetadata_DoesNotRetainInjectionTargetsOrArguments()
        {
            var owners = new Container[16];
            var references = CreateInstances(owners, false);
            try
            {
                var metadata = ReflectionCache.GetMethodInjectionData(typeof(InjectedTarget));
                MemoryRetentionAssert.Collected(new object[] { owners, metadata }, references);
            }
            finally
            {
                DisposeAll(owners);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference[] CreateInstances(Container[] containers, bool useConstructor)
        {
            var references = new WeakReference[containers.Length * 2];
            try
            {
                for (var i = 0; i < containers.Length; i++)
                {
                    var container = new Container();
                    containers[i] = container;
                    var dependency = new Dependency();
                    container.BindInstance(dependency);
                    object target;
                    if (useConstructor)
                    {
                        target = container.Instantiate<ConstructedTarget>();
                        Assert.That(((ConstructedTarget)target).Dependency, Is.SameAs(dependency));
                    }
                    else
                    {
                        var injected = new InjectedTarget();
                        container.Inject(injected);
                        Assert.That(injected.Dependency, Is.SameAs(dependency));
                        target = injected;
                    }

                    references[i * 2] = new WeakReference(dependency);
                    references[i * 2 + 1] = new WeakReference(target);
                }
            }
            finally
            {
                DisposeAll(containers);
            }

            return references;
        }

        private static void DisposeAll(Container[] containers)
        {
            foreach (var container in containers)
                container?.Dispose();
        }
    }
}
