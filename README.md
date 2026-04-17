# **RoboPatch (Alpha)**

**RoboPatch** is a custom asset framework for *Robotopia* that allows you to inject and replace in-game assets at runtime.

Built using **BepInEx** and **Harmony**, it hooks into Unity’s asset loading system and redirects it to custom content, letting you modify the game without permanently changing its files.

> ⚠️ **RoboPatch is still in alpha.** It is usable, but some features are still evolving.

---

## 🚀 Features

* Inject custom assets into *Robotopia* at runtime
* Replace existing game assets dynamically
* Load external **AssetBundles** at runtime
* Patch in-game **TextAssets** using `.txt` files
* AI prompt overrides via `prompts/` system
* Manifest-based mod loading (`manifest.json`)
* Fully reversible changes (no permanent file modification)

---

## 🎮 Current Controls

* Press **M** in-game to manually spawn assets

  * Works for mods with `spawn.mode = "manual"` in `manifest.json`

* Mods with `spawn.mode = "automatic"` will spawn automatically when the scene loads

---

## ⚙️ How It Works

RoboPatch uses:

* **BepInEx** – Unity plugin framework for modding
* **Harmony** – runtime patching system for hooking Unity asset loading

Before the game loads its assets, RoboPatch intercepts the process and substitutes custom content where defined. This allows:

* No permanent file modification
* Fully reversible changes
* Modular mod loading system
* Flexible AI + asset + code injection

---

## 📦 Installation Guide

> ⚠️ RoboPatch has not been tested on Linux or macOS.

### **Requirements:**

* Latest *Robotopia* build
* **BepInEx Bleeding Edge** version

---

### **Steps:**

1. Download *Robotopia* from Discord:
   [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)

2. Download **BepInEx Bleeding Edge**:
   [https://builds.bepinex.dev](https://builds.bepinex.dev)

3. Extract BepInEx into your *Robotopia* folder

4. Run the game once to generate required files

5. Download the latest **RoboPatch** release

6. Place RoboPatch DLL inside:

```text
/Robotopia/BepInEx/plugins/
```

---

## 📂 Mod Folder Structure (NEW SYSTEM)

Each mod now uses a **manifest-based structure**:

```text
/Robotopia
  /Mods
    /ExampleMod
      manifest.json

      bundles/
        example.bundle

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

  "asset": "ExamplePrefab",

  "spawn": {
    "mode": "manual",
    "scene": "City Streets",
    "position": [0, 1, 0]
  },

  "scriptClass": "ExampleMod.Main"
}
```

---

### **Fields explained:**

* `asset` – AssetBundle prefab name to load
* `spawn.mode` – `manual` or `automatic`
* `spawn.scene` – Scene name to spawn in
* `spawn.position` – XYZ spawn position
* `scriptClass` – Fully qualified DLL class to attach

---

## 💬 Prompt System (NEW)

RoboPatch supports AI behavior overrides using the `prompts/` folder.

### Structure:

```text
/prompts
  personality.txt
  guard_prompt.txt
  system.txt
```

### How it works:

* File name = prompt key
* RoboPatch loads these at runtime
* Overrides in-game AI TextAssets or behavior prompts

> Example: `personality.txt` can redefine robot behavior style.

---

## 📄 TextAsset Overrides

You can still override game TextAssets using `.txt` files.

### How:

1. Place `.txt` inside your mod folder (legacy system still supported):

```text
/prompts or /textassets (legacy fallback)
```

2. File name must match the in-game TextAsset name exactly.

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
* Output goes to `/bin/Release`

---

### 4. Install

Copy DLL to:

```text
/Robotopia/BepInEx/plugins/
```

---

## 🤝 Contributing

1. Fork repository
2. Make changes
3. Submit pull request

Keep changes small and focused.

---

## 🛑 Support / Issues

* Do NOT contact Robotopia devs for RoboPatch issues
* Use GitHub issues for bugs
* PRs welcome for fixes or improvements

---

## 💡 TODO

* Mod dependency system
* Prompt stacking (base + mod + scene)
* Mod enable/disable menu
* Hot reload system
* API for external tools

---

## 🙏 Credits

* Robotopia Dev Team / Tomato Cake Inc: [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)
* BepInEx: [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
* Harmony: [https://github.com/pardeike/Harmony](https://github.com/pardeike/Harmony)
* Cinematic Unity Explorer: [https://github.com/originalnicodr/CinematicUnityExplorer](https://github.com/originalnicodr/CinematicUnityExplorer)

---
