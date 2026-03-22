using System.Text.Json;
using BotPoe.Models;

namespace BotPoe.Services.Essence;

public class PoePriceEssenceService : IPoePriceEssenceService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoePriceEssenceService(HttpClient http, ILeagueService leagueService)
    {
        _http = http;
        _leagueService = leagueService;
    }

    public async Task<List<EssenceInfo>> GetAllEssencesAsync()
    {
        var essenceList = new List<EssenceInfo>();
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/itemoverview?league={league}&type=Essence";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);
            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "Unknown";
                var price = item.TryGetProperty("chaosValue", out var cv) ? cv.GetDouble() : 0;

                if (price > 0)
                {
                    essenceList.Add(new EssenceInfo
                    {
                        Name = name,
                        ChaosValue = Math.Round(price, 1)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] Essence Service : {ex.Message}");
        }

        return essenceList.OrderByDescending(e => e.ChaosValue).ToList();
    }
}