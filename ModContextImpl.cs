using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;
using RoboPatch;

// =============================================================================
//  ModContextImpl.cs  -  IModContext IMPLEMENTATION
//
//  This is the concrete implementation of the IModContext interface that
//  gets passed to every IMod.OnLoad(). Mods use this to interact
//  with RoboPatch: loading assets, logging, overriding prompts, etc.
//
//  If you want to add new methods to IModContext:
//    1. Add the method signature to RoboPatch.API/IModContext.cs
//    2. Implement it here
// =============================================================================

namespace RoboPatchMod
{

class ModContextImpl : IModContext
{
    private readonly Dictionary<string, UnityEngine.Object> _assets;
    private readonly List<AssetBundle> _bundles;
    private readonly ManualLogSource _logger;
    private readonly PromptManager _prompts;
    private bool _bundlesLoaded;

    public string Name { get; }
    public string ModFolder { get; }

    public ModContextImpl(
        string name,
        string folder,
        Dictionary<string, UnityEngine.Object> assets,
        List<AssetBundle> bundles,
        PromptManager prompts,
        ManualLogSource logger)
    {
        Name = name;
        ModFolder = folder;
        _assets = assets;
        _bundles = bundles;
        _prompts = prompts;
        _logger = logger;
    }

    // ── LOAD ASSET ───────────────────────────────────────────────────────────
    // Looks up an asset by name from the mod's loaded AssetBundles.
    // Returns null if not found or the type doesn't match.
    public T LoadAsset<T>(string name) where T : UnityEngine.Object
    {
        if (_assets.TryGetValue(name, out var obj) && obj is T t)
            return t;
        return null;
    }

    // ── LOAD BUNDLES ─────────────────────────────────────────────────────────
    // Explicitly loads all *.bundle files from assets/bundles/.
    // Must be called in OnLoad() — bundles no longer auto-load.
    public void LoadBundles()
    {
        if (_bundlesLoaded)
        {
            _logger.LogWarning($"[{Name}] LoadBundles() called more than once");
            return;
        }
        _bundlesLoaded = true;

        string path = System.IO.Path.Combine(ModFolder, "assets", "bundles");
        if (!System.IO.Directory.Exists(path))
        {
            _logger.LogWarning($"[{Name}] No assets/bundles/ folder found");
            return;
        }

        foreach (var bundleFile in System.IO.Directory.GetFiles(path, "*.bundle"))
        {
            var bundle = AssetBundle.LoadFromFile(bundleFile);
            if (bundle == null) continue;

            _bundles.Add(bundle);
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var obj = bundle.LoadAsset(assetName);
                _assets[System.IO.Path.GetFileNameWithoutExtension(assetName)] = obj;
            }
            _logger.LogInfo($"[{Name}] Loaded bundle {System.IO.Path.GetFileName(bundleFile)}");
        }
    }

    // ── READ FILE ────────────────────────────────────────────────────────────
    // Reads any file from the mod's root folder by relative path.
    // Returns null if the file doesn't exist.
    public string ReadAllText(string relativePath)
    {
        string fullPath = System.IO.Path.Combine(ModFolder, relativePath);
        if (!System.IO.File.Exists(fullPath))
        {
            _logger.LogWarning($"[{Name}] File not found: {relativePath}");
            return null;
        }
        return System.IO.File.ReadAllText(fullPath);
    }

    // ── SPAWN ASSET ──────────────────────────────────────────────────────────
    // Loads a GameObject asset and instantiates it at the given position.
    // Returns the spawned GameObject or null if the asset wasn't found.
    public GameObject SpawnAsset(string name, Vector3 position)
    {
        var prefab = LoadAsset<GameObject>(name);
        if (prefab == null)
        {
            _logger.LogWarning($"[{Name}] SpawnAsset: '{name}' not found");
            return null;
        }
        var go = Object.Instantiate(prefab, position, Quaternion.identity);
        go.name = name;
        _logger.LogInfo($"[{Name}] Spawned '{name}' at {position}");
        return go;
    }

    // ── LOGGING ──────────────────────────────────────────────────────────────
    public void LogInfo(string message) => _logger.LogInfo($"[{Name}] {message}");
    public void LogWarning(string message) => _logger.LogWarning($"[{Name}] {message}");
    public void LogError(string message) => _logger.LogError($"[{Name}] {message}");

}

}
