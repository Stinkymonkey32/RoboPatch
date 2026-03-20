using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "2.2.2")]
public class RoboPatch : BaseUnityPlugin
{
    private const string CURRENT_VERSION = "2.2.2";
    private const string VERSION_URL = "https://raw.githubusercontent.com/Stinkymonkey32/RoboPatch/main/version.xml";

    // ── TextAsset patch (MOD-BASED) ──
    private static Dictionary<string, string> textCache = new(StringComparer.OrdinalIgnoreCase);

    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.name)) return;

            if (textCache.TryGetValue(__instance.name, out string cachedText))
            {
                __result = cachedText;
            }
        }
    }

    // ── Mod system ──
    class ModContext
    {
        public string Name;
        public AssetBundle Bundle;
        public GameObject Prefab;
        public Dictionary<string, string> Config = new(StringComparer.OrdinalIgnoreCase);
        public List<Assembly> Assemblies = new();
    }

    private List<ModContext> mods = new();
    private string modsFolder;

    // ── UI ──
    private GameObject popupPanel;
    private Text popupText;
    private GameObject openButton;
    private string pendingUpdateURL;

    void Awake()
    {
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
        CreatePopupUI();

        Logger.LogInfo("RoboPatch initialized – Mod-based TextAsset overrides active.");
    }

    void Start()
    {
        string pluginDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string gameRoot = Path.GetFullPath(Path.Combine(pluginDir, "..", ".."));

        string modsUpper = Path.Combine(gameRoot, "Mods");
        string modsLower = Path.Combine(gameRoot, "mods");

        if (Directory.Exists(modsUpper))
        {
            modsFolder = modsUpper;
        }
        else if (Directory.Exists(modsLower))
        {
            modsFolder = modsLower;
        }
        else
        {
            // Default to "Mods" if neither exists
            modsFolder = modsUpper;
            Directory.CreateDirectory(modsFolder);
        }

        Logger.LogInfo($"Mods folder: {modsFolder}");

        LoadMods();

        // Run updater
        _ = CheckForUpdates();
    }

    // ── Update system ──
    private async Task CheckForUpdates()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        pendingUpdateURL = null;

        const string hardcodedUrl = "https://github.com/Stinkymonkey32/RoboPatch/releases/latest";

        try
        {
            await Task.Delay(500);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            client.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("RoboPatchUpdateChecker/1.0");

            string text = await client.GetStringAsync(VERSION_URL);
            string latestVersion = text.Trim();

            Logger.LogInfo($"Fetched latest version: '{latestVersion}'");

            if (latestVersion != CURRENT_VERSION)
                ShowUpdatePopup(latestVersion, hardcodedUrl);
            else
                ShowMessagePopup("RoboPatch is installed and up to date!");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Update check failed: {ex.Message}");
            ShowMessagePopup("Update check failed.\nNo internet?");
        }
    }

    private void ShowUpdatePopup(string newVersion, string url)
    {
        pendingUpdateURL = url;
        popupText.text = $"RoboPatch update available!\n\nCurrent: {CURRENT_VERSION}\nLatest: {newVersion}";
        if (openButton != null) openButton.SetActive(!string.IsNullOrEmpty(url));
        popupPanel.SetActive(true);
    }

    private void ShowMessagePopup(string message)
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        pendingUpdateURL = null;
        popupText.text = message;
        if (openButton != null) openButton.SetActive(false);
        popupPanel.SetActive(true);
    }

    // ── Mod loader ──
    private void LoadMods()
    {
        foreach (var modSubFolder in Directory.GetDirectories(modsFolder))
        {
            string modName = Path.GetFileName(modSubFolder);
            var mod = new ModContext { Name = modName };
            Logger.LogInfo($"Found mod folder: {modName}");

            // AssetBundles
            foreach (var bundleFile in Directory.GetFiles(modSubFolder, "*.bundle"))
            {
                try
                {
                    Logger.LogInfo($"[{modName}] Loading bundle: {bundleFile}");

                    mod.Bundle = AssetBundle.LoadFromFile(bundleFile);

                    if (mod.Bundle == null)
                    {
                        Logger.LogError($"[{modName}] Bundle FAILED to load!");
                    }
                    else
                    {
                        Logger.LogInfo($"[{modName}] Bundle loaded successfully!");

                        foreach (var name in mod.Bundle.GetAllAssetNames())
                            Logger.LogInfo($"[{modName}] Bundle asset: {name}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{modName}] Bundle load error: {ex}");
                }
            }

            // DLLs
            foreach (var dllFile in Directory.GetFiles(modSubFolder, "*.dll"))
            {
                try
                {
                    Logger.LogInfo($"[{modName}] Loading DLL: {dllFile}");
                    mod.Assemblies.Add(Assembly.LoadFrom(dllFile));
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{modName}] DLL load error: {ex}");
                }
            }

            // Config
            string configPath = Path.Combine(modSubFolder, "load.cfg");
            if (File.Exists(configPath))
            {
                LoadConfig(configPath, mod);
                Logger.LogInfo($"[{modName}] Config keys: {string.Join(", ", mod.Config.Keys)}");
            }
            else
            {
                Logger.LogWarning($"[{modName}] No load.cfg found!");
            }

            // TextAssets
            string textFolder = Path.Combine(modSubFolder, "textassets");
            if (Directory.Exists(textFolder))
            {
                foreach (var file in Directory.GetFiles(textFolder, "*.txt", SearchOption.AllDirectories))
                {
                    try
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        string text = File.ReadAllText(file);
                        textCache[key] = text;
                        Logger.LogInfo($"[{modName}] Loaded TextAsset override: {key}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"[{modName}] TextAsset load error: {ex}");
                    }
                }
            }

            mods.Add(mod);
        }
    }

    private void LoadConfig(string path, ModContext mod)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || !trimmed.Contains("=")) continue;

            var parts = trimmed.Split(new[] { '=' }, 2);
            mod.Config[parts[0].Trim()] = parts[1].Trim();
        }

        if (mod.Config.TryGetValue("asset", out string asset) && mod.Bundle != null)
        {
            mod.Prefab = mod.Bundle.LoadAsset<GameObject>(asset);
            Logger.LogInfo($"[{mod.Name}] Prefab loaded: {(mod.Prefab != null ? asset : "NULL")}");
        }
    }

    // ── Spawn ──
    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.M))
        {
            Logger.LogInfo("M key pressed!");
            foreach (var mod in mods)
            {
                SpawnMod(mod);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var mod in mods)
        {
            if (!mod.Config.TryGetValue("spawn", out string spawnMode)) continue;
            if (!spawnMode.Equals("manual", StringComparison.OrdinalIgnoreCase)) continue;

            if (mod.Config.TryGetValue("scene", out string sceneName))
            {
                if (!scene.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase)) continue;
            }

            SpawnMod(mod);
        }
    }

    private void SpawnMod(ModContext mod)
    {
        GameObject instance = null;

        if (mod.Prefab != null)
        {
            Vector3 pos = Vector3.zero;
            if (mod.Config.TryGetValue("position", out string posStr))
            {
                var p = posStr.Split(',');
                if (p.Length == 3 &&
                    float.TryParse(p[0], out float x) &&
                    float.TryParse(p[1], out float y) &&
                    float.TryParse(p[2], out float z))
                {
                    pos = new Vector3(x, y, z);
                }
            }

            Logger.LogInfo($"[{mod.Name}] Spawning prefab at {pos}");
            instance = Instantiate(mod.Prefab, pos, Quaternion.identity);
        }
        else
        {
            // Standalone placeholder if no prefab/assetbundle
            instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = mod.Name;
            Logger.LogInfo($"[{mod.Name}] Spawned standalone placeholder (no prefab).");
        }

        AttachScript(instance, mod);
    }

    private void AttachScript(GameObject obj, ModContext mod)
    {
        if (!mod.Config.TryGetValue("scriptClass", out string className)) return;

        foreach (var asm in mod.Assemblies)
        {
            var type = asm.GetType(className);
            if (type == null) continue;

            var comp = obj.AddComponent(type);
            Logger.LogInfo($"[{mod.Name}] Added component: {className}");

            var method = type.GetMethod("Activate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                try
                {
                    method.Invoke(comp, null);
                    Logger.LogInfo($"[{mod.Name}] Called Activate()");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{mod.Name}] Activate error: {ex}");
                }
            }

            break;
        }
    }

    // ── UI ──
    private void CreatePopupUI()
    {
        var canvasGO = new GameObject("RoboPatchCanvas");
        DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(canvasGO.transform, false);
        var img = popupPanel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.85f);
        var rect = popupPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(350, 150);
        rect.anchoredPosition = Vector2.zero;

        popupText = new GameObject("PopupText").AddComponent<Text>();
        popupText.transform.SetParent(popupPanel.transform, false);
        popupText.text = "RoboPatch loaded!";
        popupText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        popupText.color = Color.white;
        popupText.alignment = TextAnchor.MiddleCenter;
        popupText.fontSize = 16;

        var tRect = popupText.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(320, 80);
        tRect.anchoredPosition = new Vector2(0, 20);

        openButton = CreateButton("Open GitHub", new Vector2(-70, -50), () =>
        {
            if (!string.IsNullOrEmpty(pendingUpdateURL))
                Application.OpenURL(pendingUpdateURL);
        }, popupPanel.transform);

        CreateButton("Dismiss", new Vector2(70, -50), () =>
        {
            popupPanel.SetActive(false);
        }, popupPanel.transform);

        popupPanel.SetActive(false);
    }

    private GameObject CreateButton(string label, Vector2 pos, Action onClick, Transform parent)
    {
        var btnGO = new GameObject(label);
        btnGO.transform.SetParent(parent, false);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());

        var rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140, 40);
        rect.anchoredPosition = pos;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);

        var txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;

        var tRect = txtGO.GetComponent<RectTransform>();
        tRect.sizeDelta = rect.sizeDelta;

        return btnGO;
    }
}