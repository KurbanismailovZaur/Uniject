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
        container.Bind<ScoreService>().AsCached();
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

    [Inject]
    public void Construct(ScoreService score) => _score = score;

    public void AddPoint() => _score.Add(1);
}
```

### Isolate a subsystem with FromSubcontainerResolve

Expose an `Inventory` facade while keeping its state in a child container:

```csharp
public sealed class InventoryState
{
    public System.Collections.Generic.List<string> Items { get; } = new();
}

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
    .FromSubcontainerResolve()
    .ByMethod(child =>
    {
        child.Bind<InventoryState>().AsCached();
        child.Bind<Inventory>().AsCached();
    })
    .AsCached();
```

Consumers inject `Inventory` as usual. Its `InventoryState` binding is only available inside the child container. Dependencies missing from the child can still be resolved from its parent.

**Move the configuration into a reusable installer:**

```csharp
public sealed class InventoryInstaller : Uniject.Installers.IInstaller
{
    public void Install(Uniject.Container container)
    {
        container.Bind<InventoryState>().AsCached();
        container.Bind<Inventory>().AsCached();
    }
}
```

```csharp
container.Bind<Inventory>()
    .FromSubcontainerResolve()
    .ByInstaller<InventoryInstaller>()
    .AsCached();
```

**Use an existing container:**

```csharp
var inventoryContainer = new Container();
new InventoryInstaller().Install(inventoryContainer);

container.Bind<Inventory>()
    .FromSubcontainerResolve()
    .ByInstance(inventoryContainer)
    .AsCached();
```

Uniject builds the child before resolving `Inventory`. `ByInstance` also assigns the binding's container as its parent. Keep the existing container alive while it is in use and dispose it when its owner shuts down; unlike children created by `ByMethod` or `ByInstaller`, it is not owned by the parent.

The outer `AsCached()` reuses the **child container**. The inner `Bind<Inventory>().AsCached()` reuses the **Inventory instance**. For `ByMethod` and `ByInstaller`, changing the outer call to `AsTransient()` creates a fresh child on each resolve; `ByInstance` always uses the supplied container.

### Create and reuse objects

Define a factory and a pool for your game type:

```csharp
public sealed class Enemy
{
    public sealed class Factory : Uniject.Factory<Enemy> { }
    public sealed class Pool : Uniject.Pool<Enemy> { }
}
```

Register them inside `GameInstaller.Install`:

```csharp
container.BindFactory<Enemy, Enemy.Factory>()
    .FromConstructor().AsCached();

container.BindPool<Enemy, Enemy.Pool>()
    .WithInitialSize(16).FromConstructor().AsCached();
```

Inject `Enemy.Factory` or `Enemy.Pool` into a consumer as `factory` or `pool`, then create a new object or borrow one from the pool:

```csharp
Enemy created = factory.Create(); // A new object on each call.
Enemy reused = pool.Spawn();
pool.Despawn(reused); // Return it when finished.
```

### Connect systems with signals

Register the signal bus inside `GameInstaller.Install`. The container will dispose it along with its subscriptions:

```csharp
container.Bind<SignalBus>().AsCached().DisposeWithContainer();
```

Define a signal, then inject `SignalBus` into senders and listeners:

```csharp
public readonly struct EnemyDefeated { }
```

With an injected `SignalBus signals`, subscribe and publish without referencing the other system directly. Unsubscribe when a listener stops listening:

```csharp
System.Action<EnemyDefeated> onDefeated = _ => Debug.Log("Enemy defeated!");

signals.Subscribe(onDefeated);
signals.Fire<EnemyDefeated>();
signals.Unsubscribe(onDefeated);
```