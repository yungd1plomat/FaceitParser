using FaceitParser.Abstractions;
using FaceitParser.Models;
using System.Text.Json;

namespace FaceitParser.Services
{
    public class SteamApi : ISteamApi, IDisposable
    {
        private const string baseUrl = "https://api.steamapis.com";

        private readonly string apiKey;

        private HttpClient client { get; set; }

        public SteamApi(string apiKey)
        {
            this.apiKey = apiKey;
            client = new HttpClient();
        }

        public async Task<Inventory> GetInventory(ulong steamID)
        {
            var resp = await client.GetAsync($"{baseUrl}/steam/inventory/{steamID}/730/2?api_key={apiKey}");
            resp.EnsureSuccessStatusCode();

            var response = await resp.Content.ReadAsStreamAsync();
            return JsonSerializer.Deserialize<Inventory>(response);
        }

        public async Task<Dictionary<string, double>> GetItems()
        {
            var resp = await client.GetAsync($"{baseUrl}/market/items/730?format=compact&compact_value=avg&api_key={apiKey}");
            resp.EnsureSuccessStatusCode();

            var response = await resp.Content.ReadAsStreamAsync();
            return JsonSerializer.Deserialize<Dictionary<string, double>>(response);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
