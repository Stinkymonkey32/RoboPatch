# Mod API Reference

---

## IMod

Implement this interface in your DLL to hook into RoboPatch's lifecycle.

```csharp
public interface IMod
{
    void OnLoad(IModContext context);
    void OnUnload();
    void OnSceneLoaded(string sceneName);
    void OnUpdate();
}
```

### Lifecycle

| Method | When | Do This |
|---|---|---|
| `OnLoad` | After DLL + prompts loaded | Call `LoadBundles()`, read configs, set up state |
| `OnUnload` | RoboPatch shutting down | Clean up GameObjects, listeners, resources |
| `OnSceneLoaded` | Every scene change | Spawn prefabs, register scene-specific objects |
| `OnUpdate` | Every frame (use sparingly) | Input checks, continuous effects |

### ModPluginAttribute (optional)

```csharp
[ModPlugin("MyMod", "1.0.0")]
public class MyMod : IMod { ... }
```

---

## IModContext

Passed to `OnLoad()`. Your window into RoboPatch.

### Properties

| Member | Returns | Description |
|---|---|---|
| `Name` | `string` | Mod display name (folder name) |
| `ModFolder` | `string` | Full path to your mod's folder |

### Asset Methods

| Method | Description |
|---|---|
| `LoadAsset<T>(string name)` | Load an asset from your loaded bundles by name. Returns `null` if not found. |
| `SpawnAsset(string name, Vector3 position)` | Load a `GameObject` and instantiate it. Returns the spawned object or `null`. |
| `LoadBundles()` | Explicitly load all `*.bundle` files from `assets/bundles/`. Call once in `OnLoad()`. |

### File Methods

| Method | Description |
|---|---|
| `ReadAllText(string relativePath)` | Read any file from your mod root. Returns `null` if missing. |

### Prompt Override Methods

| Method | Description |
|---|---|
| `OverridePrompt(string key, string text)` | Override a TextAsset by name. Only use if you know what you're doing — prompt TextAssets often contain code/logic. |
| `TryGetPromptOverride(string key, out string text)` | Check if a prompt override exists for the given key. Returns `true` and sets `text` if found. |

### Logging

| Method | Description |
|---|---|
| `LogInfo(string)` | Info message to BepInEx console |
| `LogWarning(string)` | Warning message |
| `LogError(string)` | Error message |


