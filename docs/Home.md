# RoboPatch Docs

RoboPatch is a custom asset framework for *Robotopia* that lets you inject and replace in-game assets at runtime using BepInEx and Harmony.

---

## Quick Links

| Guide | What it covers |
|---|---|
| [Getting Started](Getting-Started.md) | Install RoboPatch, create your first mod |
| [Mod Structure](Mod-Structure.md) | Folder layout, `manifest.json`, file access |
| [Mod API Reference](Mod-API-Reference.md) | All `IMod` and `IModContext` methods |
| [Asset Bundle Guide](Asset-Bundle-Guide.md) | Creating and loading Unity AssetBundles |
| [Prompt System](Prompt-System.md) | Overriding AI TextAssets at runtime |
| [Troubleshooting](Troubleshooting.md) | Common issues and fixes |
| [Roadmap](Roadmap.md) | Current limitations and planned features |

---

## The Short Version

```
Robotopia/Mods/YourMod/
├── manifest.json              metadata (optional)
├── assets/bundles/*.bundle    asset bundles (loaded via code)
├── prompts/*.txt              text overrides (auto-loaded)
└── YourMod.dll                your plugin (auto-discovered)
```

```csharp
using RoboPatch;

public class MyMod : IMod
{
    private IModContext api;

    public void OnLoad(IModContext api)
    {
        api = api;
        api.LoadBundles();
        api.LogInfo("Mod loaded!");
    }

    public void OnSceneLoaded(string scene)
    {
        if (scene == "City Streets")
            api.SpawnAsset("MyPrefab", new Vector3(0, 1, 0));
    }

    public void OnUnload() { }
    public void OnUpdate() { }
}
```
