# Roadmap & Limitations

What's coming next and what the current gaps are.

---

## Current Limitations

| Gap | Impact |
|---|---|
| **No cross-mod API** | A mod can't find, talk to, or depend on another mod |
| **No hot reload** | Must restart the game to test changes |
| **No runtime toggle** | Can't enable/disable a mod mid-session |
| **`LoadBundles()` loads everything** | No way to pick specific bundles or filter asset types |
| **No config UI** | Mods have to use files or hardcoded values |

None of these are design flaws — the foundation is clean and each one can be added without breaking existing mods.

---

## Planned Features

### Hot Reload
Detect when mod files change on disk, reload bundles and DLLs without restarting the game.

### Mod Dependencies
Let mods declare dependencies in `manifest.json` and order loading automatically.

### Cross-Mod API
Add `api.FindMod(string name)` so mods can discover and interact with each other.

### Enable/Disable Menu
In-game menu to toggle individual mods on and off.

### Asset Filtering
Allow `LoadBundles("*.environment.*")` or similar to selectively load bundles.

---

## How to Help

Open issues or PRs on GitHub for any of these. The architecture is designed so each feature can be added independently.
