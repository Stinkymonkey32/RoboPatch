# **RoboPatch (Alpha)**

**RoboPatch** is a custom asset framework for *Robotopia* that allows you to inject and replace in-game assets at runtime.

Built using **BepInEx** and **Harmony**, it hooks into Unity’s asset loading system and redirects it to custom content, letting you modify the game without permanently changing its files.

> ⚠️ **RoboPatch is still in alpha.** It is usable, but some systems are still evolving.

---

## 🚀 Features

* Inject custom assets into *Robotopia* at runtime
* Replace existing game assets dynamically
* Load multiple **AssetBundles per mod**
* Patch in-game **TextAssets** using `.txt` overrides
* AI prompt override system via `prompts/`
* Manifest-based mod loading (`manifest.json`)
* Multi-scene spawn rules per mod
* Fully reversible changes (no permanent file modification)

---

## 🎮 Current Controls

* Press **M** in-game to manually spawn assets

  * Works for spawn rules with `"mode": "manual"` in `manifest.json`

* Automatic spawn rules execute on scene load when scene matches rule list

---

## ⚙️ How It Works

RoboPatch uses:

* **BepInEx** – Unity modding framework
* **Harmony** – runtime patching system

It intercepts asset loading and replaces or injects modded content dynamically, allowing:

* No permanent file modification
* Modular mod system
* Multi-asset bundle support
* Rule-based spawning system
* Runtime AI prompt modification

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

## 📂 Mod Folder Structure (CURRENT SYSTEM)

Each mod uses a **manifest-driven structure**:

```text
/Robotopia
  /Mods
    /ExampleMod
      manifest.json

      bundles/
        example.bundle
        extra.bundle

      dll/
        ExampleMod.dll

      prompts/
        personality.txt
        guard_prompt.txt
        system.txt
```

---

## 📜 Manifest System (`manifest.json`)

Example:

```json
{
  "name": "ExampleMod",
  "version": "1.0.0",

  "bundles": [
    "example.bundle",
    "extra.bundle"
  ],

  "assets": [
    {
      "name": "ExamplePrefab",
      "type": "prefab"
    }
  ],

  "spawns": [
    {
      "asset": "ExamplePrefab",
      "mode": "manual",
      "scenes": ["City Streets"],
      "position": [0, 1, 0]
    }
  ],

  "scriptClass": "ExampleMod.Main"
}
```

---

### Fields explained:

* `bundles` – List of AssetBundles to load for the mod
* `assets` – Registered asset names used for lookup
* `spawns` – List of spawn rules (supports multiple per mod)
* `asset` – Prefab name inside loaded bundles
* `mode` – `manual` or `automatic`
* `scenes` – Scenes where the spawn rule applies
* `position` – XYZ spawn position
* `scriptClass` – DLL class attached to spawned object

---

## 💬 Prompt System (AI BEHAVIOR OVERRIDES)

RoboPatch supports AI behavior modification via:

```text
/prompts
  personality.txt
  guard_prompt.txt
  system.txt
```

### How it works:

* Each file name = prompt key
* Loaded at runtime into override cache
* Overrides in-game TextAssets or AI prompts

---

## 📄 TextAsset Overrides

You can override in-game TextAssets using `.txt` files.

### How:

1. Place `.txt` files inside:

```text
/Mods/ExampleMod/prompts/
```

2. File name must match the in-game TextAsset name exactly

3. RoboPatch replaces it at runtime

---

## 🧠 Important Notes

* `textassets/` folder is deprecated
* Use `prompts/` for all text overrides
* Mods are now fully **manifest-driven**

---

## 🛠 Development / Building

### 1. Clone repo

```bash
git clone https://github.com/yourusername/RoboPatch.git
```

---

### 2. Required references

From **Robotopia_Data/Managed**:

* UnityEngine.CoreModule.dll
* UnityEngine.AssetBundleModule.dll
* UnityEngine.UI.dll
* UnityEngine.InputLegacyModule.dll

From **BepInEx/core**:

* BepInEx.Core.dll
* BepInEx.Unity.Mono.dll
* 0Harmony.dll

---

### 3. Build

* Open `.csproj` in Visual Studio / Rider
* Build solution
* Output in `/bin/Release`

---

### 4. Install

Copy DLL to:

```text
/Robotopia/BepInEx/plugins/
```

---

## 🤝 Contributing

* Fork repo
* Make changes
* Submit pull request

Keep changes small and focused.

---

## 🛑 Support / Issues

* Do NOT contact Robotopia devs for RoboPatch issues
* Use GitHub issues for bugs
* PRs welcome

---

## 💡 TODO

* Mod dependency system
* Load order / priority system
* Mod menu UI
* Hot reload for manifests
* Prompt stacking system

---

## 🙏 Credits

* Robotopia Dev Team / Tomato Cake Inc: [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)
* BepInEx: [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
* Harmony: [https://github.com/pardeike/Harmony](https://github.com/pardeike/Harmony)
* Cinematic Unity Explorer: [https://github.com/originalnicodr/CinematicUnityExplorer](https://github.com/originalnicodr/CinematicUnityExplorer)
