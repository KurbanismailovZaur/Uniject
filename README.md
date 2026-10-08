<p align="center">
  <img src="Documentation~/Images/Uniject%20Styled%20Logo.png" alt="Uniject logo" width="500">
</p>

# Uniject

Uniject is a dependency injection framework for Unity, inspired by Zenject. Build modular game systems with explicit dependencies, flexible object creation, and control over object lifetimes.

## Documentation

[Read the full Uniject documentation step by step in notion](https://app.notion.com/p/mashabela/Uniject-37ab35befbee80188725c8957681e308) for installation instructions, tutorials, and examples.


## Features

- **Constructor and method injection:** Keep dependencies explicit in C# classes and inject services into MonoBehaviours.
- **Flexible bindings:** Wire classes, existing instances, components, and prefabs with a fluent API and cached or transient lifetimes.
- **Scoped containers:** Organize dependencies with scene and GameObject contexts, parent containers, and subcontainers.
- **Factories and pools:** Create objects with injected dependencies and runtime parameters, or reuse objects and collections through pooling.
- **Lifecycle management:** Use entry points, centralized updates, and container-managed disposal.
- **Typed signals:** Connect systems through `SignalBus` without direct references between senders and listeners.

## Quick start

Add `SceneContext` and the following `GameInstaller` to the same GameObject in your scene. Save the installer as `GameInstaller.cs`. Uniject creates the services, injects their dependencies, and runs the entry point when the scene starts.

```csharp
using Uniject;
using Uniject.Installers;
using Uniject.Lifecycle;
using UnityEngine;

public sealed class GameInstaller : MonoInstaller
{
    public override void Install(Container container)
    {
        // Reuse one ScoreService instance for all consumers of this binding.
        container.Bind<ScoreService>().AsCached();

        // Let Uniject create GameStartup and call Run during scene startup.
        container.Bind<GameStartup>().AsEntryPoint();
    }
}

public sealed class ScoreService
{
    public int Value { get; private set; }
    public void Add(int points) => Value += points;
}

public sealed class GameStartup : IEntryPoint
{
    private readonly ScoreService _score;

    // Uniject resolves constructor parameters from the container.
    public GameStartup(ScoreService score) => _score = score;

    public void Run()
    {
        _score.Add(10);
        Debug.Log(_score.Value); // Prints 10 when the scene starts.
    }
}
```

### Inject into MonoBehaviours

Place this component in the same scene. With the default `SceneContext` settings, Uniject injects the shared `ScoreService` through its `[Inject]` method.

```csharp
using Uniject.Attributes;
using UnityEngine;

public sealed class ScoreButton : MonoBehaviour
{
    private ScoreService _score;

    // SceneContext calls this method and supplies the shared ScoreService.
    [Inject]
    public void Construct(ScoreService score) => _score = score;

    // Connect this method to a Unity UI Button's On Click event.
    public void AddPoint() => _score.Add(1);
}
```

### Isolate a subsystem with FromSubcontainerResolve

Expose an `Inventory` facade while keeping its state in a child container:

```csharp
public sealed class InventoryState
{
    // Each child container will hold its own inventory data.
    public System.Collections.Generic.List<string> Items { get; } = new();
}

// The parent container exposes this facade to the rest of the game.
public sealed class Inventory
{
    private readonly InventoryState _state;

    public Inventory(InventoryState state) => _state = state;
    public void Add(string item) => _state.Items.Add(item);
}
```

Choose one of these alternative bindings inside `GameInstaller.Install`.

**Configure the child container inline:**

```csharp
container.Bind<Inventory>()
    .FromSubcontainerResolve() // Resolve Inventory from a child container.
    .ByMethod(child =>
    {
        // Keep one state and one facade in this child container.
        child.Bind<InventoryState>().AsCached();
        child.Bind<Inventory>().AsCached();
    })
    .AsCached(); // Reuse the child container across resolves.
```

Consumers inject `Inventory` as usual. Its `InventoryState` binding is only available inside the child container. Dependencies missing from the child can still be resolved from its parent.

**Move the configuration into a reusable installer:**

```csharp
public sealed class InventoryInstaller : Uniject.Installers.IInstaller
{
    public void Install(Uniject.Container container)
    {
        // ByInstaller passes the child container here.
        container.Bind<InventoryState>().AsCached();
        container.Bind<Inventory>().AsCached();
    }
}
```

```csharp
container.Bind<Inventory>()
    .FromSubcontainerResolve()
    .ByInstaller<InventoryInstaller>() // Configure the child with this installer.
    .AsCached(); // Keep one configured child for this binding.
```

**Use an existing container:**

```csharp
// Prepare a container and register its inventory services.
var inventoryContainer = new Container();
new InventoryInstaller().Install(inventoryContainer);

container.Bind<Inventory>()
    .FromSubcontainerResolve()
    .ByInstance(inventoryContainer) // Use this child and assign its parent.
    .AsCached();

// The caller still owns inventoryContainer and must dispose it when finished.
```

Uniject builds the child before resolving `Inventory`. `ByInstance` also assigns the binding's container as its parent. Keep the existing container alive while it is in use and dispose it when its owner shuts down; unlike children created by `ByMethod` or `ByInstaller`, it is not owned by the parent.

The outer `AsCached()` reuses the **child container**. The inner `Bind<Inventory>().AsCached()` reuses the **Inventory instance**. For `ByMethod` and `ByInstaller`, changing the outer call to `AsTransient()` creates a fresh child on each resolve; `ByInstance` always uses the supplied container.

### Create and reuse objects

Define a factory and a pool for your game type:

```csharp
public sealed class Enemy
{
    // Inject the factory to create a new Enemy on demand.
    public sealed class Factory : Uniject.Factory<Enemy> { }

    // Inject the pool to borrow and return reusable Enemy instances.
    public sealed class Pool : Uniject.Pool<Enemy> { }
}
```

Register them inside `GameInstaller.Install`:

```csharp
// Cache the factory itself; each Create call still constructs a new Enemy.
container.BindFactory<Enemy, Enemy.Factory>()
    .FromConstructor().AsCached();

// Reuse one pool and preallocate 16 enemies when it is initialized.
container.BindPool<Enemy, Enemy.Pool>()
    .WithInitialSize(16).FromConstructor().AsCached();
```

Inject `Enemy.Factory` or `Enemy.Pool` into a consumer as `factory` or `pool`, then create a new object or borrow one from the pool:

```csharp
Enemy created = factory.Create(); // A new object on each call.
Enemy reused = pool.Spawn(); // Borrow an object; the pool creates one if empty.
pool.Despawn(reused); // Return it for reuse and stop using this reference.
```

### Connect systems with signals

Register the signal bus inside `GameInstaller.Install`. The container will dispose it along with its subscriptions:

```csharp
// Share one bus and clear its subscriptions when the container is disposed.
container.Bind<SignalBus>().AsCached().DisposeWithContainer();
```

Define a signal, then inject `SignalBus` into senders and listeners:

```csharp
// A payload-free notification identified by its C# type.
public readonly struct EnemyDefeated { }
```

With an injected `SignalBus signals`, subscribe and publish without referencing the other system directly. Unsubscribe when a listener stops listening:

```csharp
// Keep the delegate so the same listener can be removed later.
System.Action<EnemyDefeated> onDefeated = _ => Debug.Log("Enemy defeated!");

signals.Subscribe(onDefeated); // Listen for EnemyDefeated signals.
signals.Fire<EnemyDefeated>(); // Notify listeners; this prints "Enemy defeated!".
signals.Unsubscribe(onDefeated); // Stop listening when this consumer is done.
```
