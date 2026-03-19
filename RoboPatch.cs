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
    // ── Original TextAsset patch ──
    private static Dictionary<string, string> textCache = new Dictionary<string, string>();

    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.name))
                return;

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

    // ── AssetBundle loading ──
    private static GameObject loadedPrefab;           
    private static AssetBundle loadedBundle;         
    private string bundlesFolder;                     

    // Store loaded DLL assemblies
    private List<Assembly> loadedAssemblies = new List<Assembly>();
    private Dictionary<string, string> currentCfg;   // Keep current cfg for M-key spawn

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
        {
            try
            {
                Directory.CreateDirectory(bundlesFolder);
                Logger.LogInfo($"Created bundles folder: {bundlesFolder}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to create bundles folder: {ex.Message}");
            }
        }

        LoadBundleFromConfig();
    }

    private void LoadBundleFromConfig()
    {
        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string configPath = Path.Combine(dllDir, "load.cfg");

        if (!File.Exists(configPath))
        {
            Logger.LogWarning("No load.cfg found next to DLL – skipping bundle load.");
            return;
        }

        Logger.LogInfo($"Reading config: {configPath}");

        var lines = File.ReadAllLines(configPath);
        currentCfg = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("//") || !trimmed.Contains("="))
                continue;

            var parts = trimmed.Split(new[] { '=' }, 2);
            if (parts.Length != 2) continue;

            currentCfg[parts[0].Trim()] = parts[1].Trim();
        }

        if (!currentCfg.TryGetValue("bundle", out string bundleFileName) || string.IsNullOrWhiteSpace(bundleFileName))
        {
            Logger.LogError("load.cfg missing or empty 'bundle=' key.");
            return;
        }

        string bundlePath = Path.Combine(bundlesFolder, bundleFileName);

        if (!File.Exists(bundlePath))
        {
            Logger.LogError($"Bundle file not found: {bundlePath}");
            Logger.LogInfo("Tip: Place your .bundle / .unity3d file in the 'bundles' folder next to the DLL.");
            return;
        }

        try
        {
            loadedBundle = AssetBundle.LoadFromFile(bundlePath);
            if (loadedBundle == null)
            {
                Logger.LogError($"Failed to load AssetBundle from {bundlePath}");
                return;
            }

            Logger.LogInfo($"Loaded AssetBundle: {bundleFileName}");

            foreach (var assetName in loadedBundle.GetAllAssetNames())
            {
                Logger.LogDebug($"  Bundle contains: {assetName}");
            }

            if (!currentCfg.TryGetValue("asset", out string assetPath) || string.IsNullOrWhiteSpace(assetPath))
            {
                Logger.LogError("load.cfg missing or empty 'asset=' key (e.g. asset=Assets/PrefabName.prefab)");
                return;
            }

            loadedPrefab = loadedBundle.LoadAsset<GameObject>(assetPath);
            if (loadedPrefab == null)
            {
                Logger.LogError($"Failed to load asset '{assetPath}' from bundle '{bundleFileName}'");
                Logger.LogWarning("Check spelling/case, and look at the 'Bundle contains:' logs above.");
                return;
            }

            Logger.LogInfo($"Successfully loaded prefab: {assetPath}");

            // Load DLL if specified
            if (currentCfg.TryGetValue("script", out string dllName) && !string.IsNullOrWhiteSpace(dllName))
            {
                LoadDLLFromBundlesFolder(dllName);
            }

            // <-- AUTO-SPAWN REMOVED -->
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception while loading bundle: {ex}");
        }
    }

    private void LoadDLLFromBundlesFolder(string dllName)
    {
        string dllPath = Path.Combine(bundlesFolder, dllName);

        if (!File.Exists(dllPath))
        {
            Logger.LogWarning($"DLL not found in bundles folder: {dllPath}");
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

    // ── Spawn + attach + activate manually ──
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && loadedPrefab != null)
        {
            var instance = Instantiate(loadedPrefab, Vector3.zero, Quaternion.identity);
            Logger.LogInfo("Manual spawn via M key.");

            if (currentCfg != null && currentCfg.TryGetValue("scriptClass", out string className) && !string.IsNullOrWhiteSpace(className))
            {
                Type scriptType = null;
                foreach (var asm in loadedAssemblies)
                {
                    scriptType = asm.GetType(className);
                    if (scriptType != null) break;
                }

                if (scriptType != null && typeof(MonoBehaviour).IsAssignableFrom(scriptType))
                {
                    var comp = instance.GetComponent(scriptType) ?? instance.AddComponent(scriptType);

                    // Call Activate method if it exists
                    var method = scriptType.GetMethod("Activate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(comp, null);
                        Logger.LogInfo($"Activated {className} immediately after spawn.");
                    }
                    else
                    {
                        Logger.LogWarning($"No Activate() method found on {className}.");
                    }
                }
                else
                {
                    Logger.LogWarning($"Script class '{className}' not found or not a MonoBehaviour.");
                }
            }
        }
    }

    void OnDestroy()
    {
        if (loadedBundle != null)
        {
            loadedBundle.Unload(false);
            Logger.LogInfo("Unloaded AssetBundle on destroy.");
        }
    }
}