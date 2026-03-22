using System.Text.Json;
using BotPoe.Models;

namespace BotPoe.Services.Beast;

public class PoePriceBeastService : IPoePriceBeastService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoePriceBeastService(HttpClient http, ILeagueService leagueService)
    {
        _http = http;
        _leagueService = leagueService;
    }

    public async Task<List<BeastInfo>> GetAllBeastsAsync()
    {
        var beastList = new List<BeastInfo>();
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/itemoverview?league={league}&type=Beast";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);
            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "Unknown";
                var price = item.TryGetProperty("chaosValue", out var cv) ? cv.GetDouble() : 0;

                if (name.Equals("Infested Ursa", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (price > 0)
                {
                    beastList.Add(new BeastInfo
                    {
                        Name = name,
                        ChaosValue = Math.Round(price, 1)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] PoeBeast Service : {ex.Message}");
        }
        return beastList.OrderByDescending(b => b.ChaosValue).ToList();
    }
}