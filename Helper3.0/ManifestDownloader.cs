using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Helper3._0
{
    public class ManifestDownloader
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://raw.githubusercontent.com/qwe213312/k25FCdfEOoEJ42S6/main";

        public ManifestDownloader()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> DownloadManifestAsync(int appId, int depotId, ulong gid, string outputFolder)
        {
            if (gid == 0) return false;

            string fileName = $"{depotId}_{gid}.manifest";
            string url = $"{BaseUrl}/{fileName}";
            string outputPath = Path.Combine(outputFolder, fileName);

            try
            {
                // Verify if file already exists
                if (File.Exists(outputPath))
                {
                    Console.WriteLine($"[SKIP] {fileName} already exists.");
                    return true;
                }

                Console.WriteLine($"[DOWNLOADING] {url}...");
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FAILED] HTTP {response.StatusCode} for {url}");
                    return false;
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(outputPath, content);
                Console.WriteLine($"[SUCCESS] Saved to {outputPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Downloading {fileName}: {ex.Message}");
                return false;
            }
        }
    }
}
