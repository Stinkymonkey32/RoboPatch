#nullable enable
using System.Security.Cryptography.X509Certificates;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

// =============================================================================
//  RoboPatch.cs  -  MAIN ENTRY POINT
//
//  This is the BepInEx plugin that Unity loads at startup. It:
//    1. Sets up Harmony patches (TextAsset override system)
//    2. Creates the mod loader and spawn system
//    3. Handles Unity lifecycle events (Awake, Start, Update, OnDestroy)
//
//  The actual mod loading logic lives in ModLoader.cs.
//  The spawn/manifest logic lives in SpawnSystem.cs.
//  All IMod hook calls are centralized in ModLifecycle.cs.
// =============================================================================

namespace RoboPatchMod
{

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "3.0.0")]
public class RoboPatch : BaseUnityPlugin
{
    private const string CURRENT_VERSION = "3.0.0";

    // ── CORE SYSTEMS ─────────────────────────────────────────────────────────
    private PromptManager _prompts;     // Manages TextAsset prompt overrides
    private ModLoader _loader;          // Discovers and loads mods from /Mods/
    public static string modsFolder;         // Full path to the /Mods/ directory
    // The plugin DLL lives in BepInEx/plugins/, so we go up 2 levels
    // to find the game root (where Mods/ should be)
    public static string pluginDir = System.IO.Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
    public static string gameRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(pluginDir, "..", ".."));

    // ── AWAKE ────────────────────────────────────────────────────────────────
    // Unity calls this once when the plugin is first loaded.
    // We set up Harmony patches and scene load event listeners here.
    void Awake()
    {
        // Apply all [HarmonyPatch] annotations in this assembly
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();

        // Initialize the prompt override system and connect it to the
        // Harmony patch that intercepts TextAsset.get_text
        _prompts = new PromptManager(Logger);
        PromptTextAssetPatch.Initialize(_prompts);

        // Listen for scene changes so we can auto-spawn assets
        SceneManager.sceneLoaded += OnSceneLoaded;

        Logger.LogInfo($"RoboPatch v{CURRENT_VERSION} (Mod API system)");
    }

    // ── START ────────────────────────────────────────────────────────────────
    // Unity calls this after Awake, once the plugin is ready.
    // We resolve the game root, create the Mods folder, and load everything.
    void Start()
    {
        modsFolder = System.IO.Path.Combine(gameRoot, "Mods");
        System.IO.Directory.CreateDirectory(modsFolder);

        // Initialize loader
        _loader = new ModLoader(Logger, _prompts, modsFolder);

        // Load all mods from /Mods/
        _loader.LoadAll();
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────
    // Unity calls this every frame.
    // Handles the manual spawn key (M) and forwards Update to all mods.
    void Update()
    {
        // Forward Update() to every loaded mod plugin
        foreach (var mod in _loader.Mods)
            ModLifecycle.Update(mod, Logger);
    }

    // ── ON SCENE LOADED ──────────────────────────────────────────────────────
    // Forwards the event to all mod plugins so they can handle their own spawning.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_loader == null) return;
        foreach (var mod in _loader.Mods)
            ModLifecycle.SceneLoaded(mod, scene.name, Logger);
    }

    // ── ON DESTROY ───────────────────────────────────────────────────────────
    // Unity calls this when the plugin is being unloaded.
    // We clean up all mods, unload asset bundles, and fire OnUnload hooks.
    void OnDestroy()
    {
        _loader?.UnloadAll();
    }
}

}

// ── HARMONY PATCH: TextAsset Override ────────────────────────────────────────
// This patches Unity's TextAsset.get_text property getter so that any
// TextAsset whose name matches a prompt override key returns custom text.
// The prompt data is managed by PromptManager.
[HarmonyPatch(typeof(TextAsset), "get_text")]
class PromptTextAssetPatch
{
    private static PromptManager _prompts;

    // Called from RoboPatch.Awake() to connect the patch to the manager
    public static void Initialize(PromptManager prompts) => _prompts = prompts;

    public static bool TryGetSystemPromptOverride(out string? text)
    {
        if (_prompts != null)
            return _prompts.TryGetSystemPromptOverride(out text);
        text = null;
        return false;
    }

    static void Postfix(TextAsset __instance, ref string __result)
    {
        // Skip null assets and assets with no name
        if (__instance == null || string.IsNullOrEmpty(__instance.name) || _prompts == null)
            return;

        // If this TextAsset's name matches an override key, swap the text
        if (_prompts.TryGetValue(__instance.name, out string value))
            __result = value;
    }
}

// ── MANIFEST SERIALIZATION CLASSES ───────────────────────────────────────────
// These are used by the manifest.json file format for mod metadata.
// They map directly to the JSON structure described in the README.
// If you change these, update the README to match!

[System.Serializable]
public class Manifest
{
    public string name;             // Mod display name
    public string version;          // Mod version string
    public AssetDef[] assets;       // (Reserved) Individual asset definitions
    public string scriptClass;      // Fully qualified class name for legacy script attachment
}

[System.Serializable]
public class AssetDef
{
    public string name;             // Asset name
    public string type;             // Asset type string (reserved)
}


