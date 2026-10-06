# Quick Start

This sample is the runnable version of the package's `Documentation~/Uniject-Quick-Start.pdf`. It demonstrates a cached service, constructor injection, and an entry point.

## Run the sample

1. In Unity's Package Manager, select **Uniject** and import **Quick Start** from its **Samples** section.
2. Open `QuickStart.unity` from the imported sample folder under `Assets/Samples/Uniject/<version>/Quick Start`.
3. Open the **Console** window and clear any previous messages.
4. Enter Play mode. The Console should display `10` once when the scene starts.

The result is shown in the Console. This minimal scene has no camera or visual interface.

## How it works

The scene contains one GameObject, **Uniject Quick Start**, with `SceneContext` and `GameInstaller`.

- `SceneContext` uses its sibling installer when the scene starts.
- `GameInstaller` registers one cached `ScoreService` and the `GameStartup` entry point.
- Uniject passes `ScoreService` into the `GameStartup` constructor.
- `GameStartup.Run()` adds 10 points and logs the resulting score.

The three scripts use the `Uniject.Samples.QuickStart` namespace and a separate assembly that references the Uniject runtime. Their behavior matches the PDF example.

## Full documentation

[Read the Uniject documentation in Notion](https://app.notion.com/p/mashabela/Uniject-37ab35befbee80188725c8957681e308).
