// =============================================================================
//  IMod.cs  -  MOD INTERFACE
//
//  Implement this interface in your mod DLL to hook into RoboPatch's
//  lifecycle. Your class will be automatically discovered and instantiated
//  when the mod loads.
//
//  TO ADD A NEW HOOK:
//    1. Add the method signature here
//    2. Add the corresponding call + error handling in ModLifecycle.cs
//    3. Call ModLifecycle.YourMethod(mod, args) from wherever you need it
//
//  EXAMPLE:
//    [ModPlugin("MyMod", "1.0.0")]
//    public class MyMod : IMod
//    {
//        public void OnLoad(IModContext api) { ... }
//        public void OnUnload() { ... }
//        public void OnSceneLoaded(string scene) { ... }
//        public void OnUpdate() { ... }
//    }
// =============================================================================

namespace RoboPatch
{
    /// <summary>
    /// Main interface for code-driven RoboPatch mods.
    /// Implement this in your DLL and place it in /Mods/YourMod/.
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// Called after all assets, DLLs, and prompts are loaded.
        /// Use this to set up your mod: spawn objects, register hooks, etc.
        /// </summary>
        void OnLoad(IModContext context);

        /// <summary>
        /// Called when RoboPatch shuts down or reloads mods.
        /// Clean up GameObjects, event listeners, or unmanaged resources here.
        /// </summary>
        void OnUnload();

        /// <summary>
        /// Called whenever a new scene finishes loading.
        /// <paramref name="sceneName"/> is the name of the active scene.
        /// </summary>
        void OnSceneLoaded(string sceneName);

        /// <summary>
        /// Called every frame. Keep this lightweight to avoid performance issues.
        /// </summary>
        void OnUpdate();
    }
}
