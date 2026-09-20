using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Uniject.Tests.Memory
{
    internal static class MemoryRetentionAssert
    {
        // Payloads must be created in a separate NoInlining method which returns only weak references.
        // Do not inspect Target in the test before collecting: Mono may keep that temporary alive.
        public static void Collected(object owner, params WeakReference[] references)
        {
            try
            {
                for (var attempt = 0; attempt < 4; attempt++)
                {
                    Collect();
                    if (AllCollected(references))
                        return;
                }

                for (var i = 0; i < references.Length; i++)
                    Assert.That(references[i].IsAlive, Is.False, $"Reference {i} is still retained.");
            }
            finally
            {
                // A passing test must prove the live owner released the payload.
                GC.KeepAlive(owner);
            }
        }

        public static void Retained(object owner, params WeakReference[] references)
        {
            try
            {
                Collect();
                for (var i = 0; i < references.Length; i++)
                    Assert.That(references[i].IsAlive, Is.True, $"Reference {i} should still be owned.");
            }
            finally
            {
                GC.KeepAlive(owner);
            }
        }

        private static void Collect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool AllCollected(WeakReference[] references)
        {
            foreach (var reference in references)
            {
                if (reference.IsAlive)
                    return false;
            }

            return true;
        }
    }
}
