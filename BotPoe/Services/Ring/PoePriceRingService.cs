using System.Text.Json;
using BotPoe.Services.League;

namespace BotPoe.Services.Ring;

public class PoePriceRingService : IPoePriceRingService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoePriceRingService(HttpClient http, ILeagueService leagueService)
    {
        _http = http;
        _leagueService = leagueService;
    }

    public async Task<double> GetPriceRingAsync(string ringName)
    {
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/itemoverview?league={league}&type=UniqueAccessory";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);
            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                var itemName = item.GetProperty("name").GetString();
                
                if (itemName?.Equals(ringName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Utilisation explicite du namespace pour éviter l'erreur de "Symbol"
                    if (item.TryGetProperty("baseType", out var baseType) && 
                        baseType.GetString()?.Contains("Ring", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        if (item.TryGetProperty("chaosValue", out var chaosElement))
                        {
                            return Math.Round(chaosElement.GetDouble(), 1);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] PoeNinja Ring : {ex.Message}");
        }
        return 0;
    }
    public async Task<List<BotPoe.Models.RingInfo>> GetAllRingsAsync()
    {
        var ringList = new List<BotPoe.Models.RingInfo>(); 
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/itemoverview?league={league}&type=UniqueAccessory";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);
            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                var baseTypeStr = item.TryGetProperty("baseType", out var bt) ? bt.GetString() : "";

                if (baseTypeStr != null && baseTypeStr.Contains("Ring", StringComparison.OrdinalIgnoreCase))
                {
                    var name = item.GetProperty("name").GetString() ?? "Unknown";
                    var price = item.TryGetProperty("chaosValue", out var cv) ? cv.GetDouble() : 0;

                    if (price > 0)
                    {
                        ringList.Add(new BotPoe.Models.RingInfo 
                        {
                            Name = name,
                            ChaosValue = Math.Round(price, 1)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] GetAllRings : {ex.Message}");
        }

        return ringList.OrderByDescending(r => r.ChaosValue).ToList();
    }
}