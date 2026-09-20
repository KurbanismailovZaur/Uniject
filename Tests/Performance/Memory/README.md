# Memory tests

The suite separates managed allocation measurements, object lifetime assertions, and
diagnostic retained-heap estimates. It extends the existing performance and Editor test
assemblies; no additional assembly definitions are required.

## Running

- Select namespace Uniject.Tests.Performance.Memory in the Unity Test Runner for
  allocation measurements and Unity object lifetime scenarios.
- Select category MemoryRetention in EditMode for Tests/Editor/Memory
  (namespace Uniject.Tests.Memory).
- Deferred Destroy, scene unloading, and SceneLoaderMemoryTests require PlayMode.
  Those tests skip in EditMode.
- The SceneLoader test reuses
  Tests/Runtime/Fixtures/Scenes/SceneLoaderRequested.unity. Enable that scene in
  Build Settings before running the test. The test skips when the scene is unavailable
  or already loaded, and does not change Build Settings.
- Tests marked Explicit must be selected deliberately: see below.

Compare results within the same Unity version, scripting backend, architecture and
runner mode. Editor and Player baselines are separate.

## Allocation measurements

AllocationMeasurement.Run records *.AllocationsPerOperation with SampleUnit.Undefined
and returns the mean number of managed allocation events per operation. It uses
Unity's GC.Alloc ProfilerRecorder with CollectOnlyOnCurrentThread and
SumAllSamplesInFrame. Each synchronous action gets a fresh Reset/Start/Stop interval;
Stop publishes the aggregate immediately, so no frame boundary is needed. The helper
reads ProfilerRecorderSample.Count, not the number of stored samples or their Value.
Setup and cleanup run for each invocation, including warmup, and are outside the
recording interval. Cleanup runs in finally, after the recorder stops. Reporting and
assertions are outside the interval too, and the recorder is disposed even on failure.

Run in the Editor or a Development Player with ENABLE_PROFILER. Players without
profiling support skip these measurements. The helper checks the recorder against a
known allocation first; an unavailable marker or failed calibration in a profiling
build fails with a diagnostic instead of reporting false zeros. Native allocations
and asynchronous work on other threads are outside this metric. No forced GC takes
place inside allocation measurements.

This metric counts allocation events, not allocated bytes or GC collections. It
replaces the previous *.BytesPerOperation metric: GC.GetAllocatedBytesForCurrentThread
is not reliable across Unity scripting backends. Do not compare these values to old
byte baselines. GC.Alloc's Value is not an allocation-size metric, and the global
GC Allocated In Frame counter would include unrelated allocations on other threads.

An operation is one action invocation unless operationsPerIteration specifies a
batch size. Spawn/despawn tests report allocations per complete reuse cycle; pending tick
tests normalize the two-dispatch cycle to allocations per dispatch. Groups named Batch
without normalization report the whole batch.

Steady-state tests warm all relevant capacities. First-resolve, first-rental and
expansion tests create a fresh owner in setup for every invocation. Most first-use
tests still have warm runtime reflection. ReflectionCacheMemoryTests distinguishes
cache hits from Uniject cache misses with warm CLR reflection. It removes and restores
only its own fixture entries, never clears global caches. These are not measurements
of a fresh process or domain.

Direct-construction and direct-method groups provide baselines for unavoidable
object allocations. Zero-allocation assertions cover justified warmed paths such as
cached resolution, missing-binding lookup and reuse within existing pool capacity.
Creation, reflection invocation, disposal and tick enumeration record measurements
without inventing byte budgets. The current tick enumerator boxes; dispatch tests
record that cost.

## Coverage

| Area | Scenarios |
| --- | --- |
| Resolve / TryResolve | Generic and Type APIs, reference/value results, cached/transient scopes, aliases, method/getter providers, missing dependencies, parent chains, subcontainers, disposable registration, binding count |
| Injection / instantiation | Dependency count and chains, inherited injection, prepared enumerables and queues, direct baselines, prefabs with InjectTargets, AddComponent |
| Factories | Constructor, method, custom factory, resolve source, first create, value/reference parameters |
| Registration / Build / Dispose | Binding configurations, factories/pools, scaling, lazy/non-lazy, entry points, queue processing, repeated calls, owned trees, shared instances, exceptions |
| Contexts | Initialization with children, installers, hierarchy scanning, Build/Run/teardown, nearest context and GameObject parent lookup |
| Pools | Preallocation, first full batch, warmed reuse, growth policies and maximum size, adoption, active/inactive/mixed cleanup, GameObject/Component lifetime |
| Collection pools | All six collection types, value/reference elements, cold/warm rentals, simultaneous rentals, capacity buckets, retained capacity after a peak |
| Ticks | All three phases, registration/removal, interface combinations, queued changes and cancellation |
| Reflection | Constructor/method cache hits and misses, absent inject method, stable cache entry counts across repeated containers |
| Lifetime | Former payloads, cached/transient objects, method closures, queues, owned/borrowed subcontainers, exception cleanup, static pools, reflection metadata, unloaded scene containers |

## Retention and native lifetime

Editor retention tests create payloads in non-inlined helpers and keep only weak
references to them. The owner stays alive through the assertion so a test cannot pass
merely because the owner itself was collected. Positive controls verify expected
ownership before release. A bounded full-GC sequence is used only for retention.

Returning a collection should release its elements while preserving reusable storage.
CollectionPool.Clear preserves tracking of borrowed collections; Dispose releases
that tracking too. Disposed owned subcontainers can intentionally retain disposal
history until the parent is disposed. Tests respect these different boundaries.

PlayMode lifetime tests wait for deferred destruction or scene unloading, then check
the native Unity objects through Unity's null comparison. When profiling is available,
UnityObjectLifetimeMemoryTests also records native bytes for its created components
using Profiler.GetRuntimeMemorySizeLong. Those samples exclude GameObjects,
Transforms, unrelated objects and the rest of the native heap. A profiler result of
zero is treated as unavailable, not as a zero-byte footprint.

The SceneLoader lifetime test spans frames and records surviving objects after unload.
It waits for the next frame before each of up to four full-GC checks so the unload
completion stack can unwind. It reports surviving containers and services separately,
plus their total; failures identify the object kind and the load cycle (starting at 1).
Diagnostics retain only counts and text, never the weak-reference targets. The test
still requires zero survivors and a Dispose call for every cycle's service.
It does not keep an allocation recording interval open across await, which would count
unrelated work performed between frames.

## Explicit diagnostics

ManagedFootprintMemoryTests estimates retained bytes per owner from signed
GC.GetTotalMemory(true) deltas while holding 64 owners alive. It covers containers,
preallocated pools, and collection buffers after a peak and after Clear. Run these
in an otherwise idle process. Whole-heap noise can produce negative deltas; these
measurements are informational and do not assert exact budgets or leak freedom.

ContextRetentionTests.HierarchyScan_WhenQueueingThrows_DoesNotRetainDestroyedTargetPayload
is an explicit regression for a source-identified issue: the hierarchy scanner can
leave a rented collection tracked by the static pool if enqueueing a target throws.
The test swaps in an isolated pool and restores the original in finally, so failure
cannot contaminate other tests. It is expected to expose the current implementation
until that runtime path is fixed; adding this suite does not change runtime behavior.

## References

- [Unity Performance Testing: Measure.Method](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.2/manual/measure-method.html):
  the package's GC() counts allocation events, not bytes or GC collections.
- [ProfilerRecorderOptions.SumAllSamplesInFrame](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.SumAllSamplesInFrame.html):
  Unity's example of synchronous GC.Alloc event counting on the calling thread.
- [ProfilerRecorder.Reset](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.Reset.html).
- [Profiler.GetRuntimeMemorySizeLong](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Profiling.Profiler.GetRuntimeMemorySizeLong.html).
