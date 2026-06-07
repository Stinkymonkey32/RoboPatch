# Mod Structure

Every mod lives in its own subfolder under `Robotopia/Mods/`.

---

## Standard Layout

```
Robotopia/Mods/YourModId/
├── manifest.json            Mod metadata (optional)
├── assets/
│   └── bundles/
│       └── yourmod.bundle   AssetBundle files (loaded via code)
├── prompts/
│   └── personality.txt      TextAsset overrides (optional)
├── YourMod.dll              Your mod assembly (optional)
├── config.json              Any files you want at root
└── README.md                etc.
```

---

## manifest.json

Optional metadata file. Fields:

| Field | Type | Description |
|---|---|---|
| `name` | string | Mod display name |
| `version` | string | Mod version string |
| `scriptClass` | string | Legacy `Activate()` class for old-style mods |

Example:

```json
{
  "name": "MyMod",
  "version": "1.0.0",
  "scriptClass": "MyMod.Main"
}
```

No spawn rules in JSON — handle spawning in code via `OnSceneLoaded`.

---

## assets/bundles/

Place `*.bundle` files here. They are NOT loaded automatically — your mod must call `api.LoadBundles()` in `OnLoad()` to load them.

---

## prompts/

Place `*.txt` files here. Each filename becomes a TextAsset override key. Loaded automatically.

---

## DLL Placement

Your mod's `.dll` goes directly in the mod root folder (not in a `scripts/` or `dll/` subfolder). RoboPatch scans `*.dll` in the mod root.

---

## File Access

Use `api.ReadAllText("filename.json")` to read any file from your mod's root folder. Returns `null` if the file doesn't exist.
