using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "1.2.0")]
public class RoboPatch : BaseUnityPlugin
{
    // Cache for text assets
    private static Dictionary<string, string> textCache = new Dictionary<string, string>();
    
    // Cache for loaded prefabs
    private static Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    void Awake()
    {
        BepInEx.Logging.Logger.CreateLogSource("SystemPrompt").LogInfo("Patching Assets...");

        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();
    }

    // --------------------------
    // TextAsset Harmony Patch
    // --------------------------
    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.name))
                return;

            string assetName = __instance.name;

            // If already cached, return cached text
            if (textCache.TryGetValue(assetName, out string cachedText))
            {
                __result = cachedText;
                return;
            }

            string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
            string filePath = Path.Combine(dllDir, assetName + ".txt");

            if (File.Exists(filePath))
            {
                string fileText = File.ReadAllText(filePath);
                __result = fileText;

                // Cache for future reads
                textCache[assetName] = fileText;

                BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                    .LogInfo($"Redirected TextAsset '{assetName}' using {filePath}");
            }
        }
    }

    // --------------------------
    // Prefab Loader
    // --------------------------
    public static GameObject LoadPrefab(string prefabName)
    {
        // Check cache first
        if (prefabCache.TryGetValue(prefabName, out GameObject cached))
            return GameObject.Instantiate(cached);

        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string prefabPath = Path.Combine(dllDir, prefabName + ".prefab");

        if (!File.Exists(prefabPath))
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogWarning($"Prefab not found: {prefabPath}");
            return null;
        }

        // Load prefab dynamically
        GameObject prefab = UnityEngine.Object.Instantiate(LoadPrefabFromFile(prefabPath));
        if (prefab == null)
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogError($"Failed to load prefab: {prefabName}");
            return null;
        }

        // Cache the prefab (the original, not instantiated copy)
        prefabCache[prefabName] = prefab;

        BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
            .LogInfo($"Loaded prefab '{prefabName}' from {prefabPath}");

        return GameObject.Instantiate(prefab);
    }

    // --------------------------
    // Helper: Load prefab from file
    // --------------------------
    private static GameObject LoadPrefabFromFile(string path)
    {
        // Unity cannot directly load .prefab from file at runtime, 
        // so we need to load via AssetBundle or Resources.
        // We'll assume a small AssetBundle for each prefab:
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogError($"Failed to load AssetBundle at {path}");
            return null;
        }

        string prefabName = Path.GetFileNameWithoutExtension(path);
        GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
        bundle.Unload(false); // keep prefab in memory
        return prefab;
    }
}