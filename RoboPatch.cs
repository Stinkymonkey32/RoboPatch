using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "1.0.0")]
public class RoboPatch : BaseUnityPlugin
{
    // ── TextAsset patch ──
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
            string filePath = Path.Combine(dllDir, "textassets", assetName + ".txt");
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

    // ── Bundle & prefab logic ──
    private static GameObject loadedPrefab;
    private static AssetBundle loadedBundle;
    private string bundlesFolder;
    private string scriptsFolder;

    private List<Assembly> loadedAssemblies = new List<Assembly>();
    private Dictionary<string, string> currentCfg;

    // ── Canvas UI ──
    private Canvas uiCanvas;
    private GameObject popupPanel;
    private Text versionText;

    void Awake()
    {
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();
        Logger.LogInfo("RoboPatch initialized – TextAsset patch active.");

        CreatePopupUI();
    }

    void Start()
    {
        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        bundlesFolder = Path.Combine(dllDir, "bundles");
        scriptsFolder = Path.Combine(dllDir, "scripts");

        Directory.CreateDirectory(bundlesFolder);
        Directory.CreateDirectory(scriptsFolder);

        LoadBundlesAndScripts();
    }

    private void LoadBundlesAndScripts()
    {
        foreach (var bundleSubFolder in Directory.GetDirectories(bundlesFolder))
        {
            string modName = Path.GetFileName(bundleSubFolder);
            Logger.LogInfo($"Found bundle folder: {modName}");

            // Load AssetBundles
            foreach (var bundleFile in Directory.GetFiles(bundleSubFolder, "*.bundle"))
            {
                try
                {
                    loadedBundle = AssetBundle.LoadFromFile(bundleFile);
                    if (loadedBundle != null)
                        Logger.LogInfo($"Loaded AssetBundle: {bundleFile}");
                    else
                        Logger.LogError($"Failed to load AssetBundle: {bundleFile}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Exception loading bundle {bundleFile}: {ex}");
                }
            }

            // Load DLLs
            string scriptSubFolder = Path.Combine(scriptsFolder, modName);
            if (Directory.Exists(scriptSubFolder))
            {
                foreach (var dllFile in Directory.GetFiles(scriptSubFolder, "*.dll"))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(dllFile);
                        loadedAssemblies.Add(asm);
                        Logger.LogInfo($"Loaded DLL '{dllFile}' for mod '{modName}'");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to load DLL {dllFile}: {ex}");
                    }
                }
            }

            // Load config
            string configPath = Path.Combine(bundleSubFolder, "load.cfg");
            if (File.Exists(configPath))
                LoadConfig(configPath);
        }
    }

    private void LoadConfig(string configPath)
    {
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

        // Spawn-ready prefab
        if (currentCfg.TryGetValue("asset", out string assetPath) && !string.IsNullOrWhiteSpace(assetPath) && loadedBundle != null)
        {
            loadedPrefab = loadedBundle.LoadAsset<GameObject>(assetPath);
            if (loadedPrefab != null)
                Logger.LogInfo($"Prefab loaded from config: {assetPath}");
            else
                Logger.LogError($"Failed to load prefab '{assetPath}' from bundle");
        }
    }

    // ── Manual spawn (original code style) ──
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
                    var method = scriptType.GetMethod("Activate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(comp, null);
                        Logger.LogInfo($"Activated {className} immediately after spawn.");
                    }
                    else
                        Logger.LogWarning($"No Activate() method found on {className}.");
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

    // ── Popup UI ──
    private void CreatePopupUI()
    {
        var canvasGO = new GameObject("RoboPatchCanvas");
        uiCanvas = canvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var versionGO = new GameObject("VersionText");
        versionGO.transform.SetParent(canvasGO.transform, false);
        versionText = versionGO.AddComponent<Text>();
        versionText.text = "RoboPatch v1.0.0";
        versionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        versionText.color = Color.cyan;
        versionText.alignment = TextAnchor.LowerLeft;
        versionText.fontSize = 16;

        var rect = versionGO.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 0);
        rect.pivot = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(10, 10);
        rect.sizeDelta = new Vector2(200, 30);

        popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(canvasGO.transform, false);
        var panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.85f);

        var panelRect = popupPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(350, 150);
        panelRect.anchoredPosition = Vector2.zero;

        var popupTextGO = new GameObject("PopupText");
        popupTextGO.transform.SetParent(popupPanel.transform, false);
        var popupText = popupTextGO.AddComponent<Text>();
        popupText.text = "RoboPatch loaded!\nDo you want to check for updates?";
        popupText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        popupText.color = Color.white;
        popupText.alignment = TextAnchor.MiddleCenter;
        popupText.fontSize = 16;

        var textRect = popupTextGO.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(320, 80);
        textRect.anchoredPosition = new Vector2(0, 20);
    }
}