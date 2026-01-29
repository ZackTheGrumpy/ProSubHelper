using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Helper3._0
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Steam Manifest Downloader ===");
            Console.WriteLine("Automated by Helper3.0");
            Console.ResetColor();

            var steamService = new SteamService();
            var downloader = new ManifestDownloader();

            string baseOutputDir = @"C:\Users\aminu\Downloads\sedutssss\luaDenu";
            if (!Directory.Exists(baseOutputDir))
            {
                Console.WriteLine($"[WARNING] Target base folder not found at {baseOutputDir}. Creating it...");
                Directory.CreateDirectory(baseOutputDir);
            }
            
            Console.WriteLine($"Target Base Folder: {baseOutputDir}");

            while (true)
            {
                Console.Write("\nEnter AppID (0 to exit): ");
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input) || input == "0")
                    break;

                if (!int.TryParse(input, out int appId))
                {
                    Console.WriteLine("Invalid AppID.");
                    continue;
                }

                Console.WriteLine($"Fetching info for AppID {appId}...");
                var depotInfos = await steamService.GetDepotInfoByAppIdAsync(appId);

                if (depotInfos.Count == 0)
                {
                    Console.WriteLine("No depots found or API error.");
                    continue;
                }

                Console.WriteLine($"Found {depotInfos.Count} items (Depots/DLCs). Processing...");

                // Output to custom folder + AppID subdir
                string appFolder = Path.Combine(baseOutputDir, appId.ToString());
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }


                // Handle DLCs (Lazy fetch) + Depots
                // Note: The SteamService returns a list where DLCs are also items but might need recursion if they have depots.
                // However, the Python script treated DLCs primarily as things to fetch info for recursively.
                // Our current SteamService adds DLCs as items with 0 size/gid initially if they weren't fully parsed.
                // But the Python script's logic: 
                // "if dlcid -> get_depot_info_by_appid(dlc) -> extend depot_infos"
                // My current logic in SteamService puts them in the list.
                // I need to check if we need to expand them here.
                
                // Let's expand DLCs here in the main loop to keep Service clean-ish (or simple).
                var expansionList = new List<DepotInfo>();
                var processedIds = new HashSet<int>();
                
                foreach(var d in depotInfos)
                {
                    processedIds.Add(d.DepotId);
                }

                // Copy initial list to safe collection to iterate
                var initialItems = new List<DepotInfo>(depotInfos);

                foreach (var depot in initialItems)
                {
                    // If it looks like a DLC (Gid=0, Size=0, name contains DLC maybe? Or just relying on the fact it came from 'listofdlc')
                    // Actually, in ParseJson I set Gid=0 for DLCs from the 'listofdlc' section.
                    // Real depots usually have Gid != 0.
                    if (depot.Gid == 0) // It's likely a DLC placeholder or a depot without manifest.
                    {
                        // Try to fetch it as an AppID
                        Console.WriteLine($"Fetching DLC info: {depot.DepotId} ({depot.GameName})...");
                        var dlcInfos = await steamService.GetDepotInfoByAppIdAsync(depot.DepotId);
                        expansionList.AddRange(dlcInfos);
                    }
                }
                
                // Merge lists
                var finalDepots = new List<DepotInfo>();
                // Add valid depots from initial list
                foreach(var d in depotInfos) 
                {
                    if (d.Gid != 0) finalDepots.Add(d);
                }
                // Add valid depots from expansion
                foreach(var d in expansionList)
                {
                    if (d.Gid != 0 && !processedIds.Contains(d.DepotId)) 
                    {
                        finalDepots.Add(d);
                        processedIds.Add(d.DepotId);
                    }
                }

                Console.WriteLine($"Downloading manifests for {finalDepots.Count} valid depots...");

                foreach (var depot in finalDepots)
                {
                    if (depot.Gid != 0)
                    {
                        await downloader.DownloadManifestAsync(appId, depot.DepotId, depot.Gid, appFolder);
                    }
                }

                Console.WriteLine("\nDone!");
            }
        }
    }
}
