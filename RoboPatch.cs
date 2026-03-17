using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "2.0.0")]
public class RoboPatch : BaseUnityPlugin
{
    // Cache for text assets
    private static Dictionary<string, string> textCache = new Dictionary<string, string>();
    
    // Cache for loaded prefabs (always kept in memory)
    private static Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    void Awake()
    {
        BepInEx.Logging.Logger.CreateLogSource("SystemPrompt").LogInfo("Patching Assets...");

        // Patch text assets
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();

        // Preload prefabs here so they're always in memory
        PreloadPrefab("Mimicer");  // Add other prefab names here if needed
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
    // Prefab Loader (always in memory)
    // --------------------------
    public static GameObject LoadPrefab(string prefabName)
    {
        // Check cache first
        if (prefabCache.TryGetValue(prefabName, out GameObject cached))
            return GameObject.Instantiate(cached);

        BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
            .LogWarning($"Prefab '{prefabName}' not preloaded. Returning null.");
        return null;
    }

    // --------------------------
    // Preload a prefab into memory
    // --------------------------
    private static void PreloadPrefab(string prefabName)
    {
        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string bundlePath = Path.Combine(dllDir, prefabName + ".bundle"); // Must be an AssetBundle

        if (!File.Exists(bundlePath))
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogWarning($"Prefab bundle not found: {bundlePath}");
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogError($"Failed to load AssetBundle: {bundlePath}");
            return;
        }

        GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
        if (prefab == null)
        {
            BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                .LogError($"Prefab not found inside bundle: {prefabName}");
            bundle.Unload(false);
            return;
        }

        prefabCache[prefabName] = prefab;
        bundle.Unload(false); // keep prefab in memory

        BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
            .LogInfo($"Preloaded prefab '{prefabName}' from {bundlePath}");
    }

    // --------------------------
    // Optional: spawn prefab anywhere
    // --------------------------
    public static GameObject SpawnPrefab(string prefabName, Vector3 position)
    {
        GameObject instance = LoadPrefab(prefabName);
        if (instance != null)
        {
            instance.transform.position = position;
            return instance;
        }
        return null;
    }
}