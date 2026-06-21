# Mod Structure

Every mod lives in its own subfolder under `Robotopia/Mods/`.

---

## Standard Layout

```
Robotopia/Mods/YourModId/
├── manifest.json            Mod metadata (Recommended)
├── assets/
│   └── bundles/
│       └── yourmod.bundle   AssetBundle files (loaded via code)
├── prompts/
│   └── personality.txt      Prompt overrides (optional)
├── YourMod.dll              Your mod assembly (Required for advanced mods)
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

Example:

```json
{
  "name": "MyMod",
  "version": "1.0.0"
}
```
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
