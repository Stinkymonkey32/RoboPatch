using System;
using BepInEx.Logging;
using RoboPatch;

// =========================================================================
//  MOD LIFECYCLE MANAGER
//  ─────────────────────
//  ALL calls to IMod go through this class. This is the ONE file
//  you edit when adding or removing lifecycle hooks.
//
//  HOW TO ADD A NEW HOOK:
//    1. Add the method to IMod in RoboPatch.API/IMod.cs
//    2. Add a static method here that wraps the call in try/catch
//    3. Call ModLifecycle.YourMethod(mod, args) from wherever you need it
//
//  Every hook automatically isolates errors per-mod so one broken mod
//  never takes down the others.
// =========================================================================

namespace RoboPatchMod
{

static class ModLifecycle
{
    // ── ON LOAD ──────────────────────────────────────────────────────────
    // Called after a mod's bundles, DLLs, and prompts are fully loaded.
    // The mod receives its IModContext here and should set up any
    // listeners, spawn rules, or prompt overrides.
    public static void Load(LoadedMod mod, IModContext ctx, ManualLogSource logger)
    {
        if (mod.Plugin == null) return;
        try
        {
            mod.Plugin.OnLoad(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError($"[{mod.Name}] OnLoad error: {ex.Message}");
        }
    }

    // ── ON UNLOAD ────────────────────────────────────────────────────────
    // Called when RoboPatch shuts down or mods are being reloaded.
    // The mod should clean up any GameObject instances, event listeners,
    // or unmanaged resources it created.
    public static void Unload(LoadedMod mod, ManualLogSource logger)
    {
        if (mod.Plugin == null) return;
        try
        {
            mod.Plugin.OnUnload();
        }
        catch (Exception ex)
        {
            logger.LogError($"[{mod.Name}] OnUnload error: {ex.Message}");
        }
    }

    // ── ON SCENE LOADED ──────────────────────────────────────────────────
    // Called every time a new scene finishes loading.
    // sceneName is the name of the active scene (e.g. "City Streets").
    // Mods can use this to spawn entities, register objects, etc.
    public static void SceneLoaded(LoadedMod mod, string sceneName, ManualLogSource logger)
    {
        if (mod.Plugin == null) return;
        try
        {
            mod.Plugin.OnSceneLoaded(sceneName);
        }
        catch (Exception ex)
        {
            logger.LogError($"[{mod.Name}] OnSceneLoaded error: {ex.Message}");
        }
    }

    // ── ON UPDATE ────────────────────────────────────────────────────────
    // Called every frame via MonoBehaviour.Update().
    // WARNING: This runs every frame! Keep the implementation lightweight
    // to avoid performance issues.
    public static void Update(LoadedMod mod, ManualLogSource logger)
    {
        if (mod.Plugin == null) return;
        try
        {
            mod.Plugin.OnUpdate();
        }
        catch (Exception ex)
        {
            logger.LogError($"[{mod.Name}] OnUpdate error: {ex.Message}");
        }
    }
}

}
