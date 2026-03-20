---

# RoboPatch (Alpha)

**RoboPatch** is a modding framework for *Robotopia* that allows you to inject and replace in-game assets at runtime.

Built using **BepInEx** and **Harmony**, RoboPatch hooks into Unity’s asset loading system and redirects it to custom content, letting you modify the game without permanently changing its files.

> ⚠️ RoboPatch is still in **alpha**. It is usable, but may be slightly unstable and some features are manual.

---

## 🚀 Features

* Inject custom assets into Robotopia
* Replace existing game assets dynamically
* Load external **AssetBundles** at runtime
* Patch in-game **TextAssets** using `.txt` files
* Temporary changes (remove BepInEx to restore vanilla)

---

## 🎮 Current Controls

* Press **M** in-game to manually spawn assets

  * Only works for mods with `spawn=manual` in their `load.cfg`
* Mods with `spawn=automatic` will load assets automatically without pressing **M**

---

## ⚙️ How It Works

RoboPatch uses:

* **BepInEx** – a Unity plugin framework
* **Harmony** – a runtime patching library

Before the game loads its assets, RoboPatch intercepts the process and substitutes your custom assets in place of the originals. This means:

* No permanent file modification
* Fully reversible changes
* Flexible modding possibilities

---

## 📦 Installation Guide

> ⚠️ RoboPatch has not been tested on Linux or macOS.

### Requirements

* Latest *Robotopia* build
* BepInEx **Bleeding Edge**

### Steps

1. Download *Robotopia* from Discord:
   [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)

2. Download BepInEx Bleeding Edge:
   [https://builds.bepinex.dev](https://builds.bepinex.dev)

3. Extract BepInEx into your Robotopia folder

4. Run the game **once** to generate required files

5. Download the latest RoboPatch release from **Releases**

6. **Extract RoboPatch** and place the **entire RoboPatch folder** inside:

```text id="l0bh2n"
/Robotopia/BepInEx/plugins/
```

> RoboPatch will now load automatically when you start the game.

---

## 📂 Mod Folder Structure

To add a new mod to RoboPatch:

```text id="1jem7l"
/Robotopia
   /mods
      /ExampleMod
         ExampleMod.dll      ← Mod script
         example.bundle      ← AssetBundle
         load.cfg            ← Configuration file for asset/script
```

**Rules:**

* Each mod gets its **own folder** under `/mods`
* Scripts (DLLs) and AssetBundles go **directly in the mod folder**
* RoboPatch automatically loads **everything in the mod folder** on startup

---

## ⚙️ Mod Configuration (`load.cfg`)

Example `load.cfg`:

```text id="q12yk4"
# Example load.cfg
asset=example.bundle
spawn=manual
scene=City Streets
position=0,1,0
scriptClass=Example
```

**Fields explained:**

* `asset` – The AssetBundle file to load
* `spawn` – Spawn behavior (`manual` or `automatic`)
* `scene` – Scene to spawn the asset in (exact name/path required)
* `position` – Vector3 position in the scene
* `scriptClass` – The fully qualified class name inside the DLL to instantiate

> RoboPatch reads this file to know **which asset bundle to load, where to spawn it, and which mod script to run**.

**Tip:** Scene names must match exactly as they appear in Robotopia, including sub-location paths separated by slashes.

---

## 📄 Overriding TextAssets

RoboPatch allows you to **replace in-game TextAssets** using plain `.txt` files.

**How to do it:**

1. Place your `.txt` file inside the `/textassets` folder:

```text id="878pxs"
/Robotopia
   /textassets
       SystemPrompt.txt
       Bio.txt
```

2. The **file name must exactly match** the in-game TextAsset name (case-sensitive)
3. RoboPatch will automatically load these files and override the originals

**Tip for modding and contributing:**

* Use **[Cinematic Unity Explorer](https://github.com/originalnicodr/CinematicUnityExplorer)** to explore the game’s assets and find exact TextAsset names.
* This makes building mods and contributing much easier.

---

## 🛠 Development / Building

If you want to compile RoboPatch yourself or contribute:

1. **Clone the repository**:

```bash id="7fdd37"
git clone https://github.com/yourusername/RoboPatch.git
```

2. **Set up project references**:

Make sure the `.csproj` references DLLs from:

**Robotopia (`Robotopia_Data/Managed`)**:

* `UnityEngine.dll`
* `UnityEngine.CoreModule.dll`
* `UnityEngine.AssetBundleModule.dll`
* `UnityEngine.InputLegacyModule.dll`
* `UnityEngine.UI.dll`
* `UnityEngine.UIModule.dll`
* `UnityEngine.TextRenderingModule.dll`

**BepInEx (`BepInEx/core`)**:

* `BepInEx.Core.dll`
* `BepInEx.Unity.Mono.dll`
* `0Harmony.dll`

3. **Build the project**:

* Open the `.csproj` in Visual Studio, Rider, or VSCode
* Restore NuGet packages if prompted
* Build → outputs `yourmod.dll` in `/bin/Debug` or `/bin/Release`

4. **Deploy your mod**:

* Copy `yourmod.dll` into Robotopia/BepInEx/plugins/RoboPatch/mods/yourmodfolder
* Run the game, and RoboPatch will automatically load your mods and assets

---

## 🤝 Contributing

Want to help improve RoboPatch?

1. Clone the repository
2. Make your changes
3. Submit a pull request

> Keep pull requests small and descriptive — helps faster review.
> Use **Cinematic Unity Explorer** to explore game assets for building mods.

---

## 🛑 Support / Issues

RoboPatch is an **unofficial modding framework**.

* **Do not contact the Robotopia developers** for help with RoboPatch
* If you encounter bugs or issues, **open an issue** on this repository
* If you can fix a problem, **submit a pull request** so everyone benefits

---

## 💡 Suggestions / TODO

* Improve stability
* Add automatic AssetBundle reloading / UI
* Create a proper mod API
* Better documentation

---

## 🙏 Credits

* Robotopia Dev Team / Tomato Cake Inc: [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)
* BepInEx Team: [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
* Harmony: [https://github.com/pardeike/Harmony](https://github.com/pardeike/Harmony)
* Cinematic Unity Explorer: [https://github.com/originalnicodr/CinematicUnityExplorer](https://github.com/originalnicodr/CinematicUnityExplorer)

---
