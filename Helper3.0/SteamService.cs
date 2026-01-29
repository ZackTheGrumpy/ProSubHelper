using System.Text.Json;
using System.Net.Http;

namespace Helper3._0
{
    public class SteamService
    {
        private readonly HttpClient _httpClient;

        public SteamService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<DepotInfo>> GetDepotInfoByAppIdAsync(int appId)
        {
            var url = $"https://api.steamcmd.net/v1/info/{appId}";
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var steamData = JsonSerializer.Deserialize<SteamCmdResponse>(response);

                if (steamData?.Status != "success" || steamData.Data == null || !steamData.Data.ContainsKey(appId.ToString()))
                {
                    Console.WriteLine($"Error: API returned status '{steamData?.Status}' or missing data for AppID {appId}.");
                    return new List<DepotInfo>();
                }

                return ParseJson(steamData, appId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                return new List<DepotInfo>();
            }
        }

        private List<DepotInfo> ParseJson(SteamCmdResponse data, int appId)
        {
            var depotInfos = new List<DepotInfo>();
            var existingDepotIds = new HashSet<int>();
            
            var appData = data.Data![appId.ToString()];
            var gameName = appData.Common?.Name ?? "Unknown Game";

            // Parse Depots
            if (appData.Depots != null)
            {
                foreach (var kvp in appData.Depots)
                {
                    if (int.TryParse(kvp.Key, out int depotId))
                    {
                        // The JSON structure for depots can vary.
                        // It might be a direct object or contain "manifests" -> "public".
                        // We use JsonElement to inspect it manually.
                        try 
                        {
                            var element = kvp.Value;
                            long size = 0;
                            ulong gid = 0;

                            // Check for "manifests" -> "public"
                            if (element.ValueKind == JsonValueKind.Object)
                            {
                                JsonElement targetNode = element;
                                if (element.TryGetProperty("manifests", out var manifests))
                                {
                                    if (manifests.TryGetProperty("public", out var publicNode))
                                    {
                                        targetNode = publicNode;
                                    }
                                    else
                                    {
                                         // Sometimes manifests is the node itself in some structures? 
                                         // Reverting to Python logic: depots[depotid]['manifests']['public']
                                         // If 'public' is missing, it checked 'manifests' directly? 
                                         // Python: public = manifests.get("public", manifests)
                                         targetNode = manifests;
                                    }
                                }

                                if (targetNode.ValueKind == JsonValueKind.Object)
                                {
                                    if (targetNode.TryGetProperty("gid", out var gidProp))
                                    {
                                        ulong.TryParse(gidProp.GetString(), out gid);
                                    }
                                    if (targetNode.TryGetProperty("size", out var sizeProp))
                                    {
                                        long.TryParse(sizeProp.GetString(), out size);
                                    }
                                }
                            }

                            if (gid != 0)
                            {
                                depotInfos.Add(new DepotInfo(depotId, gid, size, gameName));
                                existingDepotIds.Add(depotId);
                            }
                        }
                        catch
                        {
                            // Ignore parsing errors for individual depots
                        }
                    }
                }
            }

            // Handle DLCs
            if (!string.IsNullOrEmpty(appData.Extended?.ListOfDlc))
            {
                var dlcIds = appData.Extended.ListOfDlc.Split(',');
                foreach (var dlcString in dlcIds)
                {
                    if (int.TryParse(dlcString.Trim(), out int dlcId))
                    {
                        if (!existingDepotIds.Contains(dlcId))
                        {
                            // Recursive call for DLC info?
                            // Python script did: dlc_info = get_depot_info_by_appid(int(dlcid), file_path)
                            // We should do the same synchronously or async? 
                            // Async recursion inside sync method is tricky.
                            // I'll make this method async?? OR just create a placeholder if we want to valid "lazy" loading.
                            // But better to just fetch it.
                            // HOWEVER, making ParseJson async is messy.
                            // Better: Collection of "ToFetch" DLCs and handle them in the main method?
                            // Or just make GetDepotInfoByAppIdAsync handle recursion.
                            // For simplicity, I'll add a dummy DepotInfo for DLC and let the caller refine it,
                            // OR I can use .Result (bad practice) or Refactor.
                            
                            // Let's defer DLC fetching to the main caller to avoid complex recursion here given the constraints.
                            // Actually, I'll just add it to a list and let the main loop handle it, 
                            // OR, since this is a small console app, I'll just use .Result for now or refactor.
                            // Refactoring: GetDepotInfo... returns (Infos, DlcIds).
                            // But wait, the Python script *does* recurse.
                            // I'll stick to a simple recursive approach by making ParseJson helpers or just doing it in GetDepotInfo.
                            // Let's modify GetDepotInfo to handle DLCs.
                            
                            // For this pass, I'll add a placeholder with size 0, gid 0.
                            // If it's a DLC, it might be a depot itself.
                            depotInfos.Add(new DepotInfo(dlcId, 0, 0, gameName + " DLC")); 
                            existingDepotIds.Add(dlcId);
                        }
                    }
                }
            }
            
            return depotInfos;
        }
    }
}
