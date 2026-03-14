using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using UnityEngine;
using System.IO;

[BepInPlugin("com.stinkymonkey36.robotopia_prompt_injector", "Robotopia Prompt Injector", "1.0.0")]
public class SystemPromptRedirectPlugin : BaseUnityPlugin
{
    private static string customPrompt = "Tell the player that SystemPrompt.txt is missing in the LLMPlugin folder and they need to create it for the plugin to work";

    void Awake()
    {
        // Load prompt from file if it exists, using the DLL directory
        string dllDir = Path.GetDirectoryName(typeof(SystemPromptRedirectPlugin).Assembly.Location);
        string filePath = Path.Combine(dllDir, "SystemPrompt.txt");
        if (File.Exists(filePath))
        {
            customPrompt = File.ReadAllText(filePath);
        }

        // Using static logger to avoid any "Log" vs "Logger" context errors
        BepInEx.Logging.Logger.CreateLogSource("SystemPrompt").LogInfo("Patching TextAsset.get_text...");
        
        var harmony = new Harmony("com.stinkymonkey36.robotopia_prompt_injector");
        harmony.PatchAll();
    }

    // This is the ONLY patch you need. It catches the text when the game reads it.
    [HarmonyPatch(typeof(TextAsset), "get_text")]
    class Patch_TextAsset_Text
    {
        static void Postfix(TextAsset __instance, ref string __result)
        {
            // If the asset name matches, we swap the result string
            if (__instance != null && __instance.name != null && __instance.name.Contains("SystemPrompt"))
            {
                // Log to console so you can see it working
                BepInEx.Logging.Logger.CreateLogSource("PromptPatch").LogInfo($"Redirecting TextAsset: {__instance.name}");
                __result = customPrompt;
            }
        }
    }
}