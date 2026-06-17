#nullable enable
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;

// =============================================================================
//  PromptManager.cs  -  TextAsset OVERRIDE SYSTEM
//
//  Loads prompt overrides from mods' prompts/*.txt files at startup.
//  The Harmony patch in RoboPatch.cs (PromptTextAssetPatch) reads from
//  this manager at runtime to swap TextAsset content on the fly.
// =============================================================================

class PromptManager
{
    private readonly Dictionary<string, (string modName, string text)> _overrides =
        new(System.StringComparer.OrdinalIgnoreCase);
    private readonly ManualLogSource _logger;

    private string? _systemPromptOverride;
    private string? _systemPromptOverrideModName;

    public string? SystemPromptOverride => _systemPromptOverride;

    public bool TryGetSystemPromptOverride(out string? text)
    {
        text = _systemPromptOverride;
        return _systemPromptOverride != null;
    }

    public PromptManager(ManualLogSource logger)
    {
        _logger = logger;
    }

    // ── LOAD FROM FOLDER ─────────────────────────────────────────────────────
    // Scans a mod's prompts/ folder for all .txt files.
    // File name (without extension) becomes the override key.
    // SystemPrompt.txt is routed to the server-side plan request instead
    // of overriding the client-side TextAsset.
    // If a key already exists, the latest mod to set it wins.
    public void LoadModPrompts(string modName, string promptsFolder)
    {
        if (!Directory.Exists(promptsFolder)) return;

        foreach (var file in Directory.GetFiles(promptsFolder, "*.txt"))
        {
            string key = Path.GetFileNameWithoutExtension(file);

            if (string.Equals(key, "SystemPrompt", System.StringComparison.OrdinalIgnoreCase))
            {
                string text = File.ReadAllText(file);
                if (_systemPromptOverride != null)
                    _logger.LogError($"[CONFLICT] SystemPrompt overridden by both [{_systemPromptOverrideModName}] and [{modName}]");
                _systemPromptOverride = text;
                _systemPromptOverrideModName = modName;
                continue;
            }

            if (_overrides.TryGetValue(key, out var existing))
                _logger.LogError($"[CONFLICT] Prompt '{key}' overridden by both [{existing.modName}] and [{modName}]");
            _overrides[key] = (modName, File.ReadAllText(file));
        }
    }

    // ── SET OVERRIDE ─────────────────────────────────────────────────────────
    // Allows a mod to programmatically set a prompt override at runtime.
    // Key "SystemPrompt" is treated specially — it goes to the server-side
    // plan request rather than overriding a client-side TextAsset.
    public void SetOverride(string key, string text, string modName)
    {
        if (string.Equals(key, "SystemPrompt", System.StringComparison.OrdinalIgnoreCase))
        {
            _systemPromptOverride = text;
            _systemPromptOverrideModName = modName;
            return;
        }
        _overrides[key] = (modName, text);
    }

    // ── TRY GET VALUE ────────────────────────────────────────────────────────
    // Used by the Harmony patch (PromptTextAssetPatch) to look up overrides.
    public bool TryGetValue(string key, out string? value)
    {
        if (_overrides.TryGetValue(key, out var entry))
        {
            value = entry.text;
            return true;
        }
        value = null;
        return false;
    }
}
