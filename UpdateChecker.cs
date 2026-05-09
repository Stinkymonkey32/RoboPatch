using System;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx.Logging;

// =============================================================================
//  UpdateChecker.cs  -  VERSION CHECK
//
//  Fetches the latest version from GitHub and logs a warning if RoboPatch
//  is out of date. Runs once at startup in the background.
// =============================================================================

namespace RoboPatchMod
{

class UpdateChecker
{
    private readonly ManualLogSource _logger;
    private readonly string _currentVersion;
    private const string VERSION_URL =
        "https://raw.githubusercontent.com/Stinkymonkey32/RoboPatch/main/version.xml";

    public UpdateChecker(ManualLogSource logger, string currentVersion)
    {
        _logger = logger;
        _currentVersion = currentVersion;
    }

    // Fetches version.xml and compares. Fails silently on network errors.
    public async Task Check()
    {
        try
        {
            using var client = new HttpClient();
            string latest = (await client.GetStringAsync(VERSION_URL)).Trim();
            if (latest != _currentVersion)
                _logger.LogWarning($"Update available: {latest}");
        }
        catch { }
    }
}

}
