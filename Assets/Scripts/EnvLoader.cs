using System;
using System.IO;
using UnityEngine;

public static class EnvLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadEnv()
    {
        // Path to the .env file in the root of the project (one level up from Assets)
        // Note: For a built game, the project root isn't included, so you might need a different
        // strategy for builds (e.g. putting it in StreamingAssets or passing via real env vars).
        // This is primarily for keeping keys out of GitHub during local development.
        
#if UNITY_EDITOR
        string envPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ".env");
#else
        // For builds, you could look in the current directory or StreamingAssets
        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");
#endif

        if (!File.Exists(envPath))
        {
            Debug.LogWarning($"[EnvLoader] .env file not found at {envPath}");
            return;
        }

        foreach (var line in File.ReadAllLines(envPath))
        {
            // Skip empty lines or comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            Environment.SetEnvironmentVariable(key, value);
        }

        Debug.Log("[EnvLoader] .env file loaded successfully.");
    }
}
