using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[BepInPlugin("com.stinkymonkey36.RoboPatch", "RoboPatch", "1.0.0")]
public class RoboPatch : BaseUnityPlugin
{
    // Cache to store loaded text for each asset
    private static Dictionary<string, string> textCache = new Dictionary<string, string>();

    void Awake()
    {
        BepInEx.Logging.Logger.CreateLogSource("SystemPrompt").LogInfo("Patching Assets...");

        var harmony = new Harmony("com.stinkymonkey36.RoboPatch");
        harmony.PatchAll();
    }

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
}