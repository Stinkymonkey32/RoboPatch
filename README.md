# **RoboPatch (Alpha)**

**RoboPatch** is a custom asset framework for *Robotopia* that allows you to inject and replace in-game assets at runtime.

Built using **BepInEx** and **Harmony**, it hooks into Unity's asset loading system and redirects it to custom content, letting you modify the game without permanently changing its files.

> ⚠️ **RoboPatch is still in alpha.** It is usable, but some systems are still evolving.

---

## 🚀 Features

* Inject custom assets into *Robotopia* at runtime
* Replace existing game assets dynamically
* Load multiple **AssetBundles per mod**
* Patch in-game **TextAssets** using `.txt` overrides
* AI prompt override system via `prompts/`
* **Public Mod API** (`RoboPatch.API`) for code-driven mods
* Fully reversible changes (no permanent file modification)
* Automatic conflict detection for prompt overrides

---

## ⚙️ How It Works

RoboPatch uses:

* **BepInEx** -- Unity modding framework
* **Harmony** -- runtime patching system
* **RoboPatch.API** -- public interface for code-driven mods

It intercepts asset loading and replaces or injects modded content dynamically, allowing:

* No permanent file modification
* Modular mod system
* Multi-asset bundle support
* Runtime AI prompt modification
* **Plugin-based mod API** for full control

---

## 📦 Installation Guide

> ⚠️ RoboPatch has not been tested on Linux or macOS.

### Requirements:

* Latest *Robotopia* build
* **BepInEx Bleeding Edge** version

---

### Steps:

1. Download *Robotopia* from Discord:
   [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)

2. Download **BepInEx Bleeding Edge**:
   [https://builds.bepinex.dev](https://builds.bepinex.dev)

3. Extract BepInEx into your *Robotopia* folder

4. Run the game once

5. Download RoboPatch release

6. Place DLL into:

```text
/Robotopia/BepInEx/plugins/
```

---

## 📂 Mod Folder Structure

Each mod lives in its own subfolder under `/Mods/`:

```text
/Robotopia
  /Mods
    /YourModId
      manifest.json

      assets/
        bundles/
          myassets.bundle

      YourMod.dll

      prompts/
        personality.txt
```

---

## 📜 manifest.json (Mod Metadata)

Optional metadata file for your mod:

```json
{
  "name": "YourMod",
  "version": "1.0.0",
  "scriptClass": "YourMod.Main"
}
```

### Fields:

| Field | Type | Description |
|---|---|---|
| `name` | string | Mod display name |
| `version` | string | Mod version |
| `scriptClass` | string | Fully-qualified class name for legacy script attachment |

Spawn rules are no longer defined in JSON -- handle spawning in code via `OnSceneLoaded`.

---

## 🧩 Mod API (Code-Driven Mods)

For full control, implement the `IMod` interface from `RoboPatch.API.dll`.

### 1. Reference the API

Add a reference to `RoboPatch.API.dll` in your mod project.

### 2. Implement the interface

```csharp
using RoboPatch;
using UnityEngine;

[ModPlugin("MyMod", "1.0.0")]
public class MyMod : IMod
{
    private IModContext api;

    public void OnLoad(IModContext api)
    {
        api = api;

        // Explicitly load your asset bundles
        api.LoadBundles();

        // Read config files from mod root
        string cfg = api.ReadAllText("config.json");

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

### 3. Place your DLL in the mod folder

```text
/Mods/MyMod/
  MyMod.dll          ← your mod assembly (references RoboPatch.API.dll)
  manifest.json
  assets/bundles/
    myassets.bundle
```

Your DLL is automatically discovered and instantiated. The `ModPluginAttribute` is optional but recommended for metadata.

### IMod Lifecycle

| Method | Called When |
|---|---|
| `OnLoad(IModContext)` | After DLLs and prompts are loaded (call `LoadBundles()` here) |
| `OnUnload()` | When RoboPatch shuts down or mods are reloaded |
| `OnSceneLoaded(string)` | Every time a scene loads |
| `OnUpdate()` | Every frame (use sparingly) |

### IModContext API

| Member | Description |
|---|---|
| `Name` | Mod display name (folder name) |
| `ModFolder` | Full path to the mod's folder on disk |
| `LoadAsset<T>(string)` | Load a named asset from loaded bundles |
| `SpawnAsset(string, Vector3)` | Load a GameObject and instantiate it at a position |
| `LoadBundles()` | Explicitly load all `*.bundle` files from `assets/bundles/` |
| `ReadAllText(string)` | Read any file from the mod's root folder (returns null if missing) |
| `LogInfo(string)` | Log to BepInEx console |
| `LogWarning(string)` | Log warning |
| `LogError(string)` | Log error |

---

## 💬 Prompt System (AI Behavior Overrides)

Place `*.txt` files in your mod's `prompts/` folder. The filename becomes the TextAsset override key:

```
/prompts
  personality.txt
  guard_prompt.txt
  system.txt
```

Each file is loaded at startup. If two mods override the same key, a conflict error is logged:

```
[CONFLICT] Prompt 'personality' overridden by both [ModA] and [ModB]
```

The last mod to load wins — nothing crashes.

---

## 🛠 Development / Building

### 1. Clone repo

```bash
git clone https://github.com/yourusername/RoboPatch.git
```

### 2. References required

From **Robotopia_Data/Managed**:

* UnityEngine.CoreModule.dll
* UnityEngine.AssetBundleModule.dll
* UnityEngine.UI.dll
* UnityEngine.InputLegacyModule.dll

From **BepInEx/core**:

* BepInEx.Core.dll
* BepInEx.Unity.Mono.dll
* 0Harmony.dll

### 3. Build

```bash
dotnet build RoboPatch.sln
```

Output:
* `bin/Debug/netstandard2.1/RoboPatch.dll` -- main plugin
* `RoboPatch.API/bin/Debug/netstandard2.1/RoboPatch.API.dll` -- API reference

### 4. Install

Copy `RoboPatch.dll` to:
```text
/Robotopia/BepInEx/plugins/
```

Modders: reference `RoboPatch.API.dll` in your own projects.

---

## 🤝 Contributing

* Fork repo
* Make changes
* Submit pull request

Keep changes focused and minimal.

---

## 🛑 Support / Issues

* Do NOT contact Robotopia devs for RoboPatch issues
* Use GitHub issues for bugs
* PRs welcome

---

## 💡 TODO

* Mod dependency system
* Prompt stacking (base + mod + scene)
* Mod enable/disable menu
* Hot reload system
* Mod configuration UI

---

## 🙏 Credits

* Robotopia Dev Team / Tomato Cake Inc: [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)
* BepInEx: [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
* Harmony: [https://github.com/pardeike/Harmony](https://github.com/pardeike/Harmony)
* Cinematic Unity Explorer: [https://github.com/originalnicodr/CinematicUnityExplorer](https://github.com/originalnicodr/CinematicUnityExplorer)
