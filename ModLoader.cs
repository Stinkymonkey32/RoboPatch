using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using RoboPatch;

// =============================================================================
//  ModLoader.cs  -  MOD DISCOVERY & LOADING
//
//  Scans the /Mods/ directory for mod folders. Each folder can contain:
//    - manifest.json      (optional)  Mod metadata, spawn rules, legacy script class
//    - assets/bundles/  (optional)  *.bundle files (loaded via api.LoadBundles())
//    - *.dll            (optional)  Assemblies with IMod implementations
//    - prompts/         (optional)  .txt files for TextAsset overrides
//
//  LEGACY SUPPORT:
//  Old mods with scriptClass + Activate() still work.
//  New mods should implement IMod from RoboPatch.API.
// =============================================================================

namespace RoboPatchMod
{

class ModLoader
{
    private readonly ManualLogSource _logger;
    private readonly PromptManager _prompts;
    private readonly string _modsFolder;
    private readonly List<LoadedMod> _mods = new();

    // Public read-only list of all loaded mods (for other systems to iterate)
    public IReadOnlyList<LoadedMod> Mods => _mods;

    public ModLoader(ManualLogSource logger, PromptManager prompts, string modsFolder)
    {
        _logger = logger;
        _prompts = prompts;
        _modsFolder = modsFolder;
    }

    // ── LOAD ALL MODS ────────────────────────────────────────────────────────
    // Iterates every subfolder in /Mods/ and attempts to load it as a mod.
    // Each mod is isolated: if one fails, the others still load fine.
    public void LoadAll()
    {
        foreach (var folder in Directory.GetDirectories(_modsFolder))
        {
            string modName = Path.GetFileName(folder);
            try
            {
                LoadMod(modName, folder);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{modName}] Failed to load: {ex.Message}");
            }
        }
    }

    // ── LOAD A SINGLE MOD ────────────────────────────────────────────────────
    // 1. Parse manifest.json (if present)
    // 2. Load all *.dll from the mod root folder
    // 3. Call api.LoadBundles() inside OnLoad (no longer automatic)
    // 4. Load prompt overrides from prompts/
    // 5. Discover IMod implementations in loaded assemblies
    // 6. Fire OnLoad on the discovered plugin (via ModLifecycle)
    private void LoadMod(string modName, string folder)
    {
        // ── 1. PARSE MANIFEST ───────────────────────────────────────────
        string manifestPath = Path.Combine(folder, "manifest.json");
        bool hasManifest = File.Exists(manifestPath);

        Manifest manifest = null;
        if (hasManifest)
        {
            manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
            _logger.LogInfo($"[{modName}] Loaded manifest");
        }

        // ── 2. INIT STORAGE ───────────────────────────────────────────
        var assets = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);
        var bundles = new List<AssetBundle>();
        var assemblies = new List<Assembly>();

        // Note: bundles are NOT auto-loaded anymore.
        // Mods must call api.LoadBundles() in their OnLoad() to load them.

        // ── 3. LOAD DLLs FROM MOD ROOT ─────────────────────────────────
        foreach (var dll in Directory.GetFiles(folder, "*.dll"))
        {
            try
            {
                assemblies.Add(Assembly.LoadFrom(dll));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{modName}] DLL error: {ex.Message}");
            }
        }

        // ── 4. LOAD PROMPTS ────────────────────────────────────────────
        _prompts.LoadModPrompts(modName, Path.Combine(folder, "prompts"));

        // ── 5. DISCOVER IMod ─────────────────────────────────────
        // Search all loaded assemblies for a concrete class that implements
        // IMod. Only the first found class is used per mod.
        var ctx = new ModContextImpl(modName, folder, assets, bundles, _prompts, _logger);

        IMod plugin = null;
        foreach (var asm in assemblies)
        {
            foreach (var type in asm.GetExportedTypes())
            {
                if (typeof(IMod).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    plugin = (IMod)Activator.CreateInstance(type);
                    _logger.LogInfo($"[{modName}] Initialized plugin: {type.FullName}");
                    goto found;   // Only one plugin per mod
                }
            }
        }
        found:

        // ── 6. FIRE OnLoad ─────────────────────────────────────────────
        // Uses the centralized ModLifecycle so all hook calls go through
        // one error-handling path. Edit ModLifecycle.cs to add hooks.
        ModLifecycle.Load(plugin != null
            ? new LoadedMod { Name = modName, Plugin = plugin }
            : null, ctx, _logger);

        // ── 7. REGISTER MOD ────────────────────────────────────────────
        _mods.Add(new LoadedMod
        {
            Name = modName,
            Manifest = manifest,
            Plugin = plugin,
            Assets = assets,
            Bundles = bundles,
            Assemblies = assemblies
        });

        _logger.LogInfo($"[{modName}] Loaded successfully");
    }

    // ── UNLOAD ALL MODS ──────────────────────────────────────────────────────
    // Fires OnUnload on every mod (reverse order), then unloads AssetBundles.
    public void UnloadAll()
    {
        for (int i = _mods.Count - 1; i >= 0; i--)
        {
            var mod = _mods[i];

            // Fire OnUnload through the centralized lifecycle manager
            ModLifecycle.Unload(mod, _logger);

            // Unload all AssetBundles for this mod
            foreach (var b in mod.Bundles)
                b.Unload(false);
        }
        _mods.Clear();
    }
}

}

// ── LOADED MOD DATA ──────────────────────────────────────────────────────────
// Holds everything associated with a loaded mod: manifest, plugin, assets, etc.
// This is the data structure that other systems (SpawnSystem, etc.) reference.
class LoadedMod
{
    public string Name;                                      // Mod folder name
    public Manifest Manifest;                                // Parsed manifest.json (may be null)
    public IMod Plugin;                                     // IMod instance (may be null for legacy mods)
    public Dictionary<string, UnityEngine.Object> Assets;    // Asset name -> loaded asset cache
    public List<AssetBundle> Bundles;                        // Loaded AssetBundles (for cleanup)
    public List<Assembly> Assemblies;                        // Loaded DLL assemblies (for legacy script lookup)
}
