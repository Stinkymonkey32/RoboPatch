# Prompt System

RoboPatch can override in-game prompts at runtime, letting you modify AI behavior prompts without modifying game files.

---

## How It Works

A Harmony patch intercepts `TextAsset.get_text`. When a TextAsset's name matches an override key, the patched text is returned instead of the original.

---

## Prompt Files

Place `*.txt` files in your mod's `prompts/` folder:

```
YourMod/
└── prompts/
    ├── personality.txt
    ├── guard_prompt.txt
    └── SystemPrompt.txt
```

- **File name** = override key (the TextAsset name to replace)
- **File content** = the replacement text
- Loaded automatically at mod startup

**MAKE SURE THE NAME IF THE .txt FILE MATCHES THE TEXTASSET EXACTLY, OTHERWISE YOUR MOD WILL NOT WORK!**

---

## Programmatic Overrides

Check if a prompt is currently overridden:

```csharp
if (_ctx.TryGetPromptOverride("personality", out string text))
    _ctx.LogInfo($"Current override: {text}");
```

---

## Conflict Detection

If two mods override the same prompt key, RoboPatch logs an error listing both mods:

```
[CONFLICT] Prompt 'personality' overridden by both [ModA] and [ModB]
```

The last mod to set the override wins. Nothing crashes — the error just tells you which mods are stepping on each other.

---

## Use Cases

- Change AI personality for NPCs
- Replace dialogue or quest text
- Modify system prompts for behavior tuning
- A/B test different prompt variations


