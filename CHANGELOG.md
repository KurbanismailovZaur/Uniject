# Changelog

All notable changes to Uniject are documented in this file.

## [1.1.1] - 2026-10-06

### Changed

- Remove unused usings.

## [1.1.0] - 2026-10-04

### Added

- Added a standalone `SignalBus` for synchronous, type-specific signals with `Subscribe<T>`, `Unsubscribe<T>`, and `Fire<T>` overloads, supporting classes, structs, and container-managed disposal through `DisposeWithContainer()`.
- Added `SignalBusInstaller`, a `MonoInstaller` that registers a cached `SignalBus` with container-managed disposal.
- Added editor tests for signal delivery, subscription changes, disposal, and container integration.
- Added CPU performance and managed allocation tests for signal dispatch and subscription cycles with 0, 1, 10, and 100 subscribers.

### Changed

- Renamed `IProvider<T>.HasData` to `HasObject` and `IProvider<T>.Data` to `Object`. This is a breaking API change; update provider implementations and usages to the new property names.

## [1.0.3] - 2026-10-03

### Removed

- Removed `FromComponentInHierarchy()` from factory bindings, including bindings configured through `To<T>()`.

## [1.0.2] - 2026-09-24


### Changed

- Specified Unity 6000.0.0f1 in the package compatibility metadata.

## [1.0.1] - 2026-09-24

### Added

- Declared package dependencies on Unity Test Framework (`com.unity.test-framework` 1.6.0) and Performance Testing (`com.unity.test-framework.performance` 3.2.0).

### Changed

- Specified Unity 6000.0.44f1 in the package compatibility metadata.

### Fixed

- Fixed editor test assembly configuration for installations through OpenUPM or a Git URL by marking the assembly as a test assembly.

## [1.0.0] - 2026-09-23

### Added

- Initial public release of Uniject, a dependency injection framework for Unity.
- Fluent bindings with transient and cached lifetimes, non-lazy resolution, parent containers, and subcontainers.
- Constructor and method injection through `[Inject]`, queued injection, and injection context support.
- Unity component bindings for existing objects, newly created GameObjects, prefabs, Resources, and component lookup in hierarchies.
- Scene and GameObject contexts with installer-based setup and global installers.
- Factories, parameterized factories, custom factories, object pools, and collection pools.
- Entry points, tickable lifecycle management, additive scene loading, and opt-in container-managed disposal through `DisposeWithContainer`.
- Editor, runtime, performance, and memory tests, along with package documentation and an MIT license.
