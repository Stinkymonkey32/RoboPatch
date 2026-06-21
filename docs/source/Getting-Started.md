# Getting Started

A quick walkthrough to install RoboPatch and create your first mod.

---

## Install RoboPatch

1. Download the latest `RoboPatch.zip` from [Releases](https://github.com/yourusername/RoboPatch/releases)
2. Copy the contents of the folder into the root of the Robotopia folder, make sure it's not the folder itself otherwise Robopatch won't work.
3. Launch the game once — RoboPatch creates the `Mods/` folder automatically

## Create Your First Mod

### Folder layout

```
Robotopia/Mods/MyFirstMod/
├── manifest.json
└── MyFirstMod.dll
```

### 1. Create manifest.json

```json
{
  "name": "MyFirstMod",
  "version": "1.0.0"
}
```

### 2. Write the plugin code

Create a new C# class library project, reference `RoboPatch.API.dll`:

```csharp
using RoboPatch;
using UnityEngine;

public class MyFirstMod : IMod
{
    private IModContext api;

    public void OnLoad(IModContext api)
    {
        api = api;
        api.LogInfo("Hello from MyFirstMod!");
    }

    public void OnSceneLoaded(string scene)
    {
        api.LogInfo($"Entered scene: {scene}");
    }

    public void OnUnload() { }
    public void OnUpdate() { }
}
```

### 3. Build and run

Build your project, drop the DLL into `Robotopia/Mods/MyFirstMod/`, launch the game. You should see your log messages in the BepInEx console.

---

## What Next?

- [Mod Structure](Mod-Structure) — folder layout, manifest.json fields
- [Mod API Reference](Mod-API-Reference) — all IMod + IModContext methods
- [Asset Bundle Guide](Asset-Bundle-Guide) — adding 3D models and prefabs
- [Prompt System](Prompt-System) — overriding AI behavior
