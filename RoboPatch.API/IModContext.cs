using UnityEngine;

// =============================================================================
//  IModContext.cs  -  MOD CONTEXT INTERFACE
//
//  Passed to IMod.OnLoad(). This is your mod's window into RoboPatch.
//  Use it to load assets, log messages, override prompts, and access your
//  mod's folder on disk.
//
//  TO ADD A NEW METHOD:
//    1. Add the signature here
//    2. Implement it in ModContextImpl.cs
//    3. Document it in the README
// =============================================================================

namespace RoboPatch
{
    /// <summary>
    /// Context object provided to every mod at load time.
    /// Gives mods access to RoboPatch systems: asset loading, logging, prompts.
    /// </summary>
    public interface IModContext
    {
        /// <summary>Your mod's display name (folder name).</summary>
        string Name { get; }

        /// <summary>Full path to your mod's folder on disk.</summary>
        string ModFolder { get; }

        /// <summary>
        /// Load an asset by name from any AssetBundle your mod loaded.
        /// Returns null if not found or type doesn't match.
        /// </summary>
        T LoadAsset<T>(string name) where T : UnityEngine.Object;

        /// <summary>
        /// Load a GameObject asset and instantiate it at the given position.
        /// Returns the spawned GameObject, or null if the asset wasn't found.
        /// </summary>
        GameObject SpawnAsset(string name, Vector3 position);

        /// <summary>
        /// Explicitly load all *.bundle files from assets/bundles/.
        /// Must be called in OnLoad() — bundles no longer auto-load.
        /// </summary>
        void LoadBundles();

        /// <summary>Read any file from your mod's root folder. Returns null if missing.</summary>
        string ReadAllText(string relativePath);

        /// <summary>Log an info message to the BepInEx console.</summary>
        void LogInfo(string message);

        /// <summary>Log a warning to the BepInEx console.</summary>
        void LogWarning(string message);

        /// <summary>Log an error to the BepInEx console.</summary>
        void LogError(string message);

        /// <summary>
        /// Override a TextAsset by name. Only use this if you know what
        /// you're doing — prompt TextAssets often contain code/logic.
        /// </summary>
        void OverridePrompt(string key, string text);

        /// <summary>
        /// Check if a prompt override exists for the given key.
        /// </summary>
        bool TryGetPromptOverride(string key, out string text);
    }
}
