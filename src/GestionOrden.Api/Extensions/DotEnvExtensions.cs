using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace GestionOrden.Api.Extensions;

public static class DotEnvExtensions
{
    /// <summary>
    /// Loads environment variables from a .env file into process environment variables.
    /// Checks several likely locations if filePath is not provided.
    /// </summary>
    public static void LoadDotEnv(string? filePath = null)
    {
        var candidates = new List<string?>();
        if (!string.IsNullOrEmpty(filePath)) candidates.Add(filePath);

        candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, ".env"));
        candidates.Add(Path.Combine(AppContext.BaseDirectory, "..", ".env"));

        foreach (var candidate in candidates.Where(c => !string.IsNullOrEmpty(c)))
        {
            var path = candidate!;
            if (!File.Exists(path)) continue;

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) continue;

                var idx = line.IndexOf('=');
                if (idx <= 0) continue;

                var key = line.Substring(0, idx).Trim();
                var val = line.Substring(idx + 1).Trim();

                // Remove surrounding quotes if present
                if ((val.StartsWith("\"") && val.EndsWith("\"")) || (val.StartsWith("'") && val.EndsWith("'")))
                {
                    val = val.Substring(1, val.Length - 2);
                }

                try
                {
                    Environment.SetEnvironmentVariable(key, val);
                }
                catch
                {
                    // ignore individual failures
                }
            }

            // stop after first found file
            return;
        }
    }
}
