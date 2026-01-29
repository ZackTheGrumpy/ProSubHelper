using System.Text.Json;
using System.Text.Json.Serialization;

namespace Helper3._0
{
    public class DepotInfo
    {
        public int DepotId { get; set; }
        public ulong Gid { get; set; }
        public long Size { get; set; }
        // We no longer need DecryptionKey for the GitHub manifest method, 
        // as we are downloading pre-decrypted/processed manifests or just the manifest files directly.
        // But if the lua script needs a key, we might need to check how the Python script obtained it.
        // User said: "instead of fetching manifest local, i want it download from outside..."
        // The original script fetched keys from local config.vdf.
        // User didn't say to SKIP keys, but the GitHub method usually implies we have the manifest.
        // However, the Lua script `addappid(depotid, 1, "key")` needs a key.
        // If the GitHub manifests are just files, we might still need the key if the game uses encryption.
        // BUT, the user's specific request "use snippet of this code" implies keeping what's useful.
        // In the new flow, we fetch info from API. The API doesn't provide keys.
        // The Python script got keys from LOCAL steam config.
        // I will keep the DecryptionKey property just in case we need it or find a way to get it later,
        // or if we strictly follow the new requirement which might bypass the need for keys if the manifest provides everything needed
        // (though typically Steam still needs a key to mount a depot unless it's open).
        // For now, I'll keep it but it might remain empty if we don't scan local config.
        // Wait, the user said "use snippet... fetch info from API... download manifest from outside".
        // Use logic regarding keys from the original script if possible? 
        // The original script scanned local config for keys.
        // I should probably keep the local key scanning if the user has the game installed,
        // but the prompt "download from outside" suggests they might NOT have it installed fully?
        // "Usage: 1. Download and start game once using a real account." -> This implies local config IS available.
        // So I should ported the key-finding logic too? The user said "use snippet of this code...".
        // I will assume I should PORT the key finding logic (Registry/VDF) as well, 
        // but the FETCHING of info comes from API.
        
        public string DecryptionKey { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;

        public DepotInfo(int depotId, ulong gid = 0, long size = 0, string gameName = "")
        {
            DepotId = depotId;
            Gid = gid;
            Size = size;
            GameName = gameName;
        }
    }

    // SteamCMD API Response Models
    public class SteamCmdResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public Dictionary<string, AppData>? Data { get; set; }
    }

    public class AppData
    {
        [JsonPropertyName("common")]
        public CommonData? Common { get; set; }

        [JsonPropertyName("depots")]
        public Dictionary<string, JsonElement>? Depots { get; set; }

        [JsonPropertyName("extended")]
        public ExtendedData? Extended { get; set; }
    }

    public class CommonData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class ExtendedData
    {
        [JsonPropertyName("listofdlc")]
        public string ListOfDlc { get; set; } = string.Empty;
    }

    public class ManifestNode
    {
        [JsonPropertyName("gid")]
        public string Gid { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;
    }
}
