using System;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Helper3._0
{
    public static class SteamUtils
    {
        [SupportedOSPlatform("windows")]
        public static string? GetSteamPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key != null)
                {
                    var path = key.GetValue("SteamPath") as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        // Registry saves path with forward slashes usually, normalize it
                        return path.Replace("/", "\\");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Reading registry: {ex.Message}");
            }
            return null;
        }
    }
}
