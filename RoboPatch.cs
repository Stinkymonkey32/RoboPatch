using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "1.0.0")]
public class RoboPatch : BaseUnityPlugin
{
    private static Dictionary<string, string> textCache = new Dictionary<string, string>();

    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.name)) return;

            string assetName = __instance.name;
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
                textCache[assetName] = fileText;

                BepInEx.Logging.Logger.CreateLogSource("RoboPatch")
                    .LogInfo($"Redirected TextAsset '{assetName}' using {filePath}");
            }
        }
    }

    private static GameObject loadedPrefab;
    private static AssetBundle loadedBundle;
    private string bundlesFolder;
    private List<Assembly> loadedAssemblies = new List<Assembly>();

    void Awake()
    {
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();
        Logger.LogInfo("RoboPatch initialized – TextAsset patch active. Loading bundle config...");
    }

    void Start()
    {
        bundlesFolder = Path.Combine(Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location), "bundles");
        if (!Directory.Exists(bundlesFolder))
            Directory.CreateDirectory(bundlesFolder);

        LoadBundleFromConfig();
    }

    private void LoadBundleFromConfig()
    {
        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string configPath = Path.Combine(dllDir, "load.cfg");
        if (!File.Exists(configPath))
        {
            Logger.LogWarning("No load.cfg found – skipping bundle load.");
            return;
        }

        var lines = File.ReadAllLines(configPath);
        var cfg = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("//") || !trimmed.Contains("=")) continue;
            var parts = trimmed.Split(new[] { '=' }, 2);
            cfg[parts[0].Trim()] = parts[1].Trim();
        }

        if (!cfg.TryGetValue("bundle", out string bundleFileName)) return;
        string bundlePath = Path.Combine(bundlesFolder, bundleFileName);
        if (!File.Exists(bundlePath))
        {
            Logger.LogError($"Bundle not found: {bundlePath}");
            return;
        }

        loadedBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedBundle == null)
        {
            Logger.LogError($"Failed to load bundle: {bundleFileName}");
            return;
        }

        Logger.LogInfo($"Loaded AssetBundle: {bundleFileName}");

        if (!cfg.TryGetValue("asset", out string assetPath)) return;
        loadedPrefab = loadedBundle.LoadAsset<GameObject>(assetPath);
        if (loadedPrefab == null)
        {
            Logger.LogError($"Failed to load prefab: {assetPath}");
            return;
        }

        Logger.LogInfo($"Successfully loaded prefab: {assetPath}");

        // Load DLL if present
        if (cfg.TryGetValue("script", out string dllName) && !string.IsNullOrWhiteSpace(dllName))
            LoadDLLFromBundlesFolder(dllName);

        // Auto-spawn
        if (cfg.TryGetValue("spawn", out string spawnStr) && bool.TryParse(spawnStr, out bool shouldSpawn) && shouldSpawn)
            SpawnLoadedPrefab(cfg);
    }

    private void LoadDLLFromBundlesFolder(string dllName)
    {
        string dllPath = Path.Combine(bundlesFolder, dllName);
        if (!File.Exists(dllPath))
        {
            Logger.LogWarning($"DLL not found: {dllPath}");
            return;
        }

        try
        {
            var asm = Assembly.LoadFrom(dllPath);
            loadedAssemblies.Add(asm);
            Logger.LogInfo($"Loaded DLL: {dllName}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to load DLL {dllName}: {ex}");
        }
    }

    private void SpawnLoadedPrefab(Dictionary<string, string> cfg)
    {
        if (loadedPrefab == null) return;

        var instance = Instantiate(loadedPrefab, Vector3.zero, Quaternion.identity);
        instance.SetActive(true);
        Logger.LogInfo("Auto-spawned prefab.");

        // Attach script from DLL
        if (cfg.TryGetValue("scriptClass", out string className) && !string.IsNullOrWhiteSpace(className))
        {
            Type foundType = null;
            foreach (var asm in loadedAssemblies)
            {
                foundType = asm.GetType(className);
                if (foundType != null) break;
            }

            if (foundType != null && typeof(Component).IsAssignableFrom(foundType))
            {
                var comp = instance.AddComponent(foundType);
                // If Mimicer has Activate() method, call it immediately to start
                MethodInfo mi = foundType.GetMethod("Activate", BindingFlags.Instance | BindingFlags.Public);
                mi?.Invoke(comp, null);
                Logger.LogInfo($"Attached & activated script: {className}");
            }
            else Logger.LogWarning($"Script class not found: {className}");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && loadedPrefab != null)
        {
            var instance = Instantiate(loadedPrefab, Vector3.zero, Quaternion.identity);
            instance.SetActive(true);
            Logger.LogInfo("Manual spawn via M key.");
        }
    }

    void OnDestroy()
    {
        loadedBundle?.Unload(false);
        Logger.LogInfo("Unloaded AssetBundle.");
    }
}