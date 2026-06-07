# Troubleshooting

Common issues and how to fix them.

---

## "Mod loaded successfully" but nothing happens

**Probable cause:** You didn't call `LoadBundles()` in `OnLoad()`.

Bundles are not loaded automatically. Your mod must explicitly call:

```csharp
public void OnLoad(IModContext api)
{
    api = api;
    api.LoadBundles();  // ← don't forget this
}
```

---

## `SpawnAsset()` returns null

**Check:**

1. Did you call `LoadBundles()`? (see above)
2. Is the bundle file in `assets/bundles/` with a `.bundle` extension?
3. Does the asset name match exactly (case-insensitive)?
4. Is the asset actually a `GameObject` prefab?

---

## `ReadAllText()` returns null

**Check:**

1. The file exists in your mod's root folder
2. The path is relative (e.g. `"config.json"` not `"C:/..."`)
3. The path uses forward slashes (`"subfolder/file.txt"`)

---

## `[CONFLICT]` errors in the console

Two or more mods are overriding the same prompt key. The error shows which mods are conflicting:

```
[CONFLICT] Prompt 'personality' overridden by both [ModA] and [ModB]
```

The last mod to load wins. To fix: agree on shared prompt keys with other mod authors, or use different key names.

---

## Mod DLL not detected

**Check:**

1. The DLL is directly in the mod root folder (not in a subfolder)
2. The DLL references `RoboPatch.API.dll`
3. Your class is `public` and implements `IMod`
4. The class has a parameterless constructor (or no constructor at all)

---

## BepInEx console shows nothing from my mod

**Check:**

1. `RoboPatch.dll` is in `BepInEx/plugins/`
2. Console is visible (BepInEx settings)
3. Your mod's `manifest.json` is valid JSON
4. The `Mods/` folder exists next to the game executable

---

## Still stuck?

Open a [GitHub issue](https://github.com/yourusername/RoboPatch/issues) with:
- Your mod's folder layout
- The full BepInEx console log
- Your `manifest.json`
- Your `OnLoad()` code
