using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Uniject.Tests.Performance.Memory
{
    public class SceneLoaderMemoryTests
    {
        private const string SceneName = "SceneLoaderRequested";
        private const string ScenePath =
            "Packages/com.codomaster.uniject/Tests/Runtime/Fixtures/Scenes/SceneLoaderRequested.unity";

        [Test, Performance]
        public async Task LoadUnload_Repeatedly_ReleasesSceneContainersAndServices()
        {
            if (!Application.isPlaying)
                Assert.Ignore("SceneLoader lifetime measurements require PlayMode.");
            if (SceneUtility.GetBuildIndexByScenePath(ScenePath) < 0)
                Assert.Ignore("Enable the existing SceneLoaderRequested fixture scene in Build Settings.");
            if (SceneManager.GetSceneByName(SceneName).isLoaded)
                Assert.Ignore("The fixture scene is already loaded; this test must own its scene.");

            const int cycles = 8;
            using var parent = new Container();
            var loader = new SceneLoader(parent);
            var references = new WeakReference[cycles * 2];
            var counter = new DisposeCounter();
            for (var i = 0; i < cycles; i++)
            {
                try
                {
                    // The async operation spans frames. A per-thread allocation delta across await
                    // would include unrelated game/test-runner work, so only lifetime is checked here.
                    await LoadAndCapture(loader, references, i * 2, counter);
                }
                finally
                {
                    var scene = SceneManager.GetSceneByName(SceneName);
                    if (scene.IsValid() && scene.isLoaded)
                        await SceneManager.UnloadSceneAsync(scene);
                }
            }

            try
            {
                const int collectionAttempts = 4;
                var survivors = (Containers: 0, Services: 0, Description: string.Empty);
                for (var attempt = 0; attempt < collectionAttempts; attempt++)
                {
                    // Let the unload completion stack unwind before testing collectability.
                    // Repeated GC calls on that same stack cannot release its temporary references.
                    await Awaitable.NextFrameAsync();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    survivors = CountSurvivors(references);
                    if (survivors.Containers + survivors.Services == 0)
                        break;
                }
                var live = survivors.Containers + survivors.Services;
                Measure.Custom(new SampleGroup("SceneLoader.SurvivingObjects", SampleUnit.Undefined), live);
                Measure.Custom(new SampleGroup("SceneLoader.SurvivingContainers", SampleUnit.Undefined), survivors.Containers);
                Measure.Custom(new SampleGroup("SceneLoader.SurvivingServices", SampleUnit.Undefined), survivors.Services);
                Assert.That(counter.Count, Is.EqualTo(cycles));
                Assert.That(live, Is.Zero,
                    $"Scene objects survived unloading after {collectionAttempts} collection attempts on separate frames. " +
                    "Remaining: " + survivors.Description);
            }
            finally
            {
                GC.KeepAlive(loader);
                GC.KeepAlive(parent);
            }
        }

        private static async Task LoadAndCapture(SceneLoader loader, WeakReference[] references,
            int offset, DisposeCounter counter)
        {
            await loader.LoadSceneAdditiveAsync(SceneName, installMethod: container =>
            {
                var payload = new Payload(counter);
                container.BindInstance(payload).DisposeWithContainer();
                references[offset] = new WeakReference(container);
                references[offset + 1] = new WeakReference(payload);
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (int Containers, int Services, string Description) CountSurvivors(WeakReference[] references)
        {
            var containers = 0;
            var services = 0;
            var description = new StringBuilder();
            for (var i = 0; i < references.Length; i++)
            {
                var reference = references[i];
                var cycle = i / 2 + 1;
                Assert.That(reference, Is.Not.Null, $"The SceneLoader install callback was not invoked for cycle {cycle}.");
                // Do not read Target into diagnostics: that could keep the observed object alive.
                if (!reference.IsAlive)
                    continue;

                var isContainer = i % 2 == 0;
                if (isContainer)
                    containers++;
                else
                    services++;

                if (description.Length > 0)
                    description.Append(", ");
                description.Append(isContainer ? "Container" : "Service (Payload)")
                    .Append(" from cycle ").Append(cycle);
            }
            return (containers, services, description.ToString());
        }

        private sealed class DisposeCounter { public int Count; }

        private sealed class Payload : IDisposable
        {
            private readonly DisposeCounter _counter;
            public Payload(DisposeCounter counter) => _counter = counter;
            public void Dispose() => _counter.Count++;
        }
    }
}
