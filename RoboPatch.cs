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

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "2.3.0")]
public class RoboPatch : BaseUnityPlugin
{
    private const string CURRENT_VERSION = "2.3.0";
    private const string VERSION_URL =
        "https://raw.githubusercontent.com/Stinkymonkey32/RoboPatch/main/version.xml";

    // ── PROMPTS ──
    private static Dictionary<string, string> promptCache =
        new(StringComparer.OrdinalIgnoreCase);

    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.name)) return;

            if (promptCache.TryGetValue(__instance.name, out string value))
                __result = value;
        }
    }

    // ── MANIFEST STRUCTURES ──
    [Serializable]
    public class Manifest
    {
        public string name;
        public string version;

        public string[] bundles;
        public AssetDef[] assets;
        public SpawnRule[] spawns;

        public string scriptClass;
    }

    [Serializable]
    public class AssetDef
    {
        public string name;
        public string type;
    }

    [Serializable]
    public class SpawnRule
    {
        public string asset;
        public string mode;
        public string[] scenes;
        public float[] position;
    }

    // ── MOD CONTEXT ──
    class ModContext
    {
        public string Name;
        public Manifest Manifest;

        public List<AssetBundle> Bundles = new();
        public List<Assembly> Assemblies = new();

        public Dictionary<string, UnityEngine.Object> AssetMap =
            new(StringComparer.OrdinalIgnoreCase);
    }

    private List<ModContext> mods = new();
    private string modsFolder;

    // ── UNITY ──
    void Awake()
    {
        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
        Logger.LogInfo("RoboPatch v2.3.0 (new manifest system)");
    }

    void Start()
    {
        string pluginDir = Path.GetDirectoryName(typeof(RoboPatch).Assembly.Location);
        string gameRoot = Path.GetFullPath(Path.Combine(pluginDir, "..", ".."));

        modsFolder = Path.Combine(gameRoot, "Mods");
        Directory.CreateDirectory(modsFolder);

        LoadMods();
        _ = CheckForUpdates();
    }

    // ── MOD LOADING ──
    private void LoadMods()
    {
        foreach (var folder in Directory.GetDirectories(modsFolder))
        {
            string modName = Path.GetFileName(folder);
            var mod = new ModContext { Name = modName };

            string manifestPath = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Logger.LogWarning($"[{modName}] Missing manifest.json");
                continue;
            }

            mod.Manifest = JsonUtility.FromJson<Manifest>(
                File.ReadAllText(manifestPath)
            );

            Logger.LogInfo($"[{modName}] Loaded manifest");

            // ── LOAD BUNDLES ──
            if (mod.Manifest.bundles != null)
            {
                foreach (var b in mod.Manifest.bundles)
                {
                    string path = Path.Combine(folder, b);
                    var bundle = AssetBundle.LoadFromFile(path);

                    if (bundle != null)
                    {
                        mod.Bundles.Add(bundle);
                        Logger.LogInfo($"[{modName}] Loaded bundle {b}");

                        // cache assets
                        foreach (var assetName in bundle.GetAllAssetNames())
                        {
                            var obj = bundle.LoadAsset(assetName);
                            string cleanName = Path.GetFileNameWithoutExtension(assetName);
                            mod.AssetMap[cleanName] = obj;
                        }
                    }
                }
            }

            // ── LOAD DLLs ──
            foreach (var dll in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    mod.Assemblies.Add(Assembly.LoadFrom(dll));
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[{modName}] DLL error: {ex}");
                }
            }

            // ── LOAD PROMPTS ──
            string prompts = Path.Combine(folder, "prompts");
            if (Directory.Exists(prompts))
            {
                foreach (var file in Directory.GetFiles(prompts, "*.txt"))
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    promptCache[key] = File.ReadAllText(file);
                }
            }

            mods.Add(mod);
        }
    }

    // ── SPAWN SYSTEM ──
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var mod in mods)
                RunSpawns(mod, SceneManager.GetActiveScene().name);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var mod in mods)
            RunSpawns(mod, scene.name);
    }

    private void RunSpawns(ModContext mod, string sceneName)
    {
        var spawns = mod.Manifest?.spawns;
        if (spawns == null) return;

        foreach (var spawn in spawns)
        {
            if (spawn.mode == "manual" && !Input.GetKeyDown(KeyCode.M))
                continue;

            if (spawn.scenes != null &&
                Array.IndexOf(spawn.scenes, sceneName) < 0)
                continue;

            if (!mod.AssetMap.TryGetValue(spawn.asset, out var obj))
            {
                Logger.LogWarning($"[{mod.Name}] Missing asset: {spawn.asset}");
                continue;
            }

            Vector3 pos = Vector3.zero;
            if (spawn.position != null && spawn.position.Length == 3)
                pos = new Vector3(spawn.position[0], spawn.position[1], spawn.position[2]);

            GameObject go = Instantiate(obj as GameObject, pos, Quaternion.identity);
            go.name = spawn.asset;

            AttachScript(go, mod);
        }
    }

    private void AttachScript(GameObject obj, ModContext mod)
    {
        if (string.IsNullOrEmpty(mod.Manifest.scriptClass)) return;

        foreach (var asm in mod.Assemblies)
        {
            var type = asm.GetType(mod.Manifest.scriptClass);
            if (type == null) continue;

            var comp = obj.AddComponent(type);

            var method = type.GetMethod("Activate",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            method?.Invoke(comp, null);

            Logger.LogInfo($"[{mod.Name}] Script attached: {mod.Manifest.scriptClass}");
            break;
        }
    }

    // ── UPDATE CHECK (unchanged) ──
    private async Task CheckForUpdates()
    {
        try
        {
            using var client = new HttpClient();
            string latest = (await client.GetStringAsync(VERSION_URL)).Trim();

            if (latest != CURRENT_VERSION)
                Logger.LogWarning($"Update available: {latest}");
        }
        catch { }
    }
}