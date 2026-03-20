using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "1.2.0")]
public class RoboPatch : BaseUnityPlugin
{
    private const string CURRENT_VERSION = "1.2.0";
    private const string VERSION_URL = "https://raw.githubusercontent.com/Stinkymonkey32/RoboPatch/main/version.txt";

    // ───────── Mod Context ─────────
    class ModContext
    {
        public string Name;
        public AssetBundle Bundle;
        public GameObject Prefab;
        public Dictionary<string, string> Config = new(StringComparer.OrdinalIgnoreCase);
        public List<Assembly> Assemblies = new();
    }

    private List<ModContext> mods = new();

    private string bundlesFolder;
    private string scriptsFolder;

    // ───────── UI ─────────
    private GameObject popupPanel;
    private Text popupText;
    private string pendingUpdateURL;

    void Awake()
    {
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;

        CreatePopupUI();

        Logger.LogInfo("RoboPatch initialized.");
    }

    void Start()
    {
        string dllDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        bundlesFolder = Path.Combine(dllDir, "bundles");
        scriptsFolder = Path.Combine(dllDir, "scripts");

        Directory.CreateDirectory(bundlesFolder);
        Directory.CreateDirectory(scriptsFolder);

        LoadBundlesAndScripts();

        _ = CheckForUpdates(); // 🔥 async update check
    }

    // ───────── UPDATE SYSTEM ─────────

    private async Task CheckForUpdates()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            string text = await client.GetStringAsync(VERSION_URL);

            string latestVersion = null;
            string url = null;

            foreach (var line in text.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.Contains("=")) continue;

                var parts = trimmed.Split('=');
                if (parts[0] == "version")
                    latestVersion = parts[1];
                else if (parts[0] == "url")
                    url = parts[1];
            }

            Logger.LogInfo($"Update check → Current: {CURRENT_VERSION}, Latest: {latestVersion}");

            if (latestVersion != null && IsNewerVersion(latestVersion, CURRENT_VERSION))
            {
                ShowUpdatePopup(latestVersion, url);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Update check failed: {ex.Message}");
        }
    }

    private bool IsNewerVersion(string latest, string current)
    {
        try
        {
            return new Version(latest) > new Version(current);
        }
        catch { return false; }
    }

    private void ShowUpdatePopup(string newVersion, string url)
    {
        pendingUpdateURL = url;

        if (popupText != null)
        {
            popupText.text =
                $"Update Available!\n\nCurrent: {CURRENT_VERSION}\nLatest: {newVersion}";
        }

        popupPanel.SetActive(true);
    }

    // ───────── LOAD SYSTEM ─────────

    private void LoadBundlesAndScripts()
    {
        foreach (var bundleSubFolder in Directory.GetDirectories(bundlesFolder))
        {
            string modName = Path.GetFileName(bundleSubFolder);
            var mod = new ModContext { Name = modName };

            Logger.LogInfo($"[{modName}] Loading mod...");

            foreach (var bundleFile in Directory.GetFiles(bundleSubFolder, "*.bundle"))
            {
                try
                {
                    mod.Bundle = AssetBundle.LoadFromFile(bundleFile);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{modName}] Bundle error: {ex}");
                }
            }

            string scriptSubFolder = Path.Combine(scriptsFolder, modName);
            if (Directory.Exists(scriptSubFolder))
            {
                foreach (var dllFile in Directory.GetFiles(scriptSubFolder, "*.dll"))
                {
                    try
                    {
                        mod.Assemblies.Add(Assembly.LoadFrom(dllFile));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"[{modName}] DLL error: {ex}");
                    }
                }
            }

            string configPath = Path.Combine(bundleSubFolder, "load.cfg");
            if (File.Exists(configPath))
                LoadConfig(configPath, mod);

            mods.Add(mod);
        }
    }

    private void LoadConfig(string path, ModContext mod)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || !trimmed.Contains("="))
                continue;

            var parts = trimmed.Split(new[] { '=' }, 2);
            mod.Config[parts[0].Trim()] = parts[1].Trim();
        }

        if (mod.Config.TryGetValue("asset", out string asset) && mod.Bundle != null)
        {
            mod.Prefab = mod.Bundle.LoadAsset<GameObject>(asset);
        }
    }

    // ───────── SPAWNING ─────────

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var mod in mods)
                SpawnMod(mod);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var mod in mods)
        {
            if (!mod.Config.TryGetValue("spawn", out string spawnMode))
                continue;

            if (!spawnMode.Equals("auto", StringComparison.OrdinalIgnoreCase))
                continue;

            if (mod.Config.TryGetValue("scene", out string sceneName))
            {
                if (!scene.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            SpawnMod(mod);
        }
    }

    private void SpawnMod(ModContext mod)
    {
        if (mod.Prefab == null) return;

        Vector3 pos = Vector3.zero;

        if (mod.Config.TryGetValue("position", out string posStr))
        {
            var p = posStr.Split(',');
            if (p.Length == 3)
            {
                pos = new Vector3(
                    float.Parse(p[0]),
                    float.Parse(p[1]),
                    float.Parse(p[2])
                );
            }
        }

        var instance = Instantiate(mod.Prefab, pos, Quaternion.identity);
        AttachScript(instance, mod);
    }

    private void AttachScript(GameObject obj, ModContext mod)
    {
        if (!mod.Config.TryGetValue("scriptClass", out string className))
            return;

        foreach (var asm in mod.Assemblies)
        {
            var type = asm.GetType(className);
            if (type == null) continue;

            var comp = obj.AddComponent(type);

            var method = type.GetMethod("Activate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                try { method.Invoke(comp, null); }
                catch (Exception ex)
                {
                    Logger.LogError($"[{mod.Name}] Activate error: {ex}");
                }
            }

            break;
        }
    }

    // ───────── UI ─────────

    private void CreatePopupUI()
    {
        var canvasGO = new GameObject("RoboPatchCanvas");
        DontDestroyOnLoad(canvasGO);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        popupPanel = new GameObject("PopupPanel");
        popupPanel.transform.SetParent(canvasGO.transform, false);

        var img = popupPanel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.9f);

        var rect = popupPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420, 220);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        // TEXT
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(popupPanel.transform, false);

        popupText = textGO.AddComponent<Text>();
        popupText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        popupText.fontSize = 18;
        popupText.alignment = TextAnchor.MiddleCenter;
        popupText.color = Color.white;

        var tRect = textGO.GetComponent<RectTransform>();
        tRect.sizeDelta = new Vector2(380, 120);
        tRect.anchoredPosition = new Vector2(0, 40);

        // BUTTONS
        CreateButton("Open GitHub", new Vector2(-80, -70), () =>
        {
            if (!string.IsNullOrEmpty(pendingUpdateURL))
                Application.OpenURL(pendingUpdateURL);
        }, popupPanel.transform);

        CreateButton("Dismiss", new Vector2(80, -70), () =>
        {
            popupPanel.SetActive(false);
        }, popupPanel.transform);

        popupPanel.SetActive(false);
    }

    private void CreateButton(string label, Vector2 pos, Action onClick, Transform parent)
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
    }
}