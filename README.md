# RoboPatch (Alpha)

**RoboPatch** is a modding framework for *Robotopia* that allows you to inject and replace in-game assets at runtime.

Built using **BepInEx** and **Harmony**, RoboPatch hooks into Unity’s asset loading system and redirects it to custom content, letting you modify the game without permanently changing its files.

> ⚠️ RoboPatch is still in **alpha**. It is usable, but may be slightly unstable and some features are manual.

---

## 🚀 Features

* Inject custom assets into Robotopia
* Replace existing game assets dynamically
* Load external **AssetBundles** at runtime
* Temporary changes (remove BepInEx to restore vanilla)

---

## 🎮 Current Controls

* Press **M** in-game to manually load AssetBundles

> This is temporary and will be improved in future updates.

---

## ⚙️ How It Works

RoboPatch uses:

* **BepInEx** – a Unity plugin framework
* **Harmony** – a runtime patching library

Before the game loads its assets, RoboPatch intercepts the process and substitutes your custom assets in place of the originals.

This means:

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

```text id="k8v0pq"
/Robotopia/BepInEx/plugins/
```

> RoboPatch will now load automatically when you start the game. Press **M** in-game to load your AssetBundles.

---

## 🧪 Current State

RoboPatch is **functional and usable**, but still evolving.

Limitations:

* AssetBundles must be loaded manually (press **M**)
* Some instability may occur
* Limited tooling/UI

---

## 📂 Mod Folder Structure

To add a new mod to RoboPatch:

```text id="t2q5hk"
/RoboPatch
   /bundles
      /ExampleMod
         example.bundle      ← Your AssetBundle(s)
         load.cfg             ← Mandatory configuration file
   /scripts
      ExampleMod.dll         ← Mod scripts (must match the mod folder name)
```

**Rules:**

* Each mod gets its **own folder** under `/bundles`
* AssetBundles go inside that folder
* A **`load.cfg` file is mandatory** for every mod
* Mod scripts must be a **compiled DLL** in `/scripts`

  * The DLL **name must match the mod folder name**
  * e.g., `ExampleMod.dll` inside `/RoboPatch/scripts/`
* Press **M** in-game to load all AssetBundles

---

## ⚙️ Mod Configuration (`load.cfg`)

Every AssetBundle must have a corresponding `load.cfg` in the same folder as the bundle. Example:

```text id="f6r7kl"
/RoboPatch
   /bundles
      /ExampleMod
         example.bundle
         load.cfg
   /scripts
      ExampleMod.dll
```

Example `load.cfg` content:

```text id="b2q9sj"
bundle = example.bundle
asset = Assets/Example.prefab
script = ExampleMod.dll
scriptClass = RoboPatch.Example
```

**Fields explained:**

* `bundle` – The AssetBundle file to load
* `asset` – Path to the asset inside the bundle (e.g., prefab path)
* `script` – The compiled DLL for the mod (from `/scripts`)
* `scriptClass` – The fully qualified class name inside the DLL that RoboPatch should instantiate

> RoboPatch uses this file to know **which assets to load and which mod scripts to execute**.
> **`load.cfg` is mandatory for every mod**, even if it only contains one bundle.

---

## 🛠 Development / Building

If you want to compile RoboPatch yourself or contribute:

1. **Clone the repository**:

```bash id="1p2y04"
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

> 💡 Hint: Place all DLLs in your project root or adjust `<HintPath>` in `RoboPatch.csproj` accordingly.

3. **Build the project**:

* Open the `.csproj` in Visual Studio, Rider, or VSCode
* Restore NuGet packages if prompted
* Build → outputs `RoboPatch.dll` in `/bin/Debug` or `/bin/Release`

4. **Deploy your mod**:

* Copy `RoboPatch.dll` into your Robotopia + BepInEx `plugins` folder
* Run the game, press **M**, enjoy your mod

---

## 🤝 Contributing

Want to help improve RoboPatch?

1. Clone the repository
2. Make your changes
3. Submit a pull request

GitHub guide:
[https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository)

### Dev Notes

* You need `.dll` files from BepInEx and Robotopia as mentioned above
* Make pull requests small and descriptive — helps faster review

---

## 🛑 Support / Issues

RoboPatch is an **unofficial modding framework**.

* **Do not contact the Robotopia developers** for help with RoboPatch
* If you encounter bugs or issues, **open an issue** on this repository
* If you can fix a problem, **submit a pull request** so everyone benefits

> This helps keep support organized and ensures the official game devs aren’t bothered with modding issues.

---

## 💡 Suggestions / TODO

* Improve stability
* Add automatic AssetBundle loading
* Create a proper mod API
* Better documentation
* UI for managing mods

---

## 🙏 Credits

* Robotopia Dev Team / Tomato Cake Inc: [https://discord.gg/5vQvxFNDGJ](https://discord.gg/5vQvxFNDGJ)
* BepInEx Team: [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
* Harmony: [https://github.com/pardeike/Harmony](https://github.com/pardeike/Harmony)
* Cinematic Unity Explorer: [https://github.com/originalnicodr/CinematicUnityExplorer](https://github.com/originalnicodr/CinematicUnityExplorer)

---

yeah there is some AI-generated code in here
i’m learning C# as I go 👍

---
