using System.Text.Json;
using BotPoe.Models;
using BotPoe.Services.League;

namespace BotPoe.Services.belt;

public class PoePriceBeltService : IPoePriceBeltService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoePriceBeltService(HttpClient http, ILeagueService leagueService)
    {
        _http = http;
        _leagueService = leagueService;
    }

    public async Task<double> GetPriceBeltAsync(string beltName)
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
                if (item.GetProperty("name").GetString()?.Equals(beltName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (item.TryGetProperty("baseType", out var baseType) && baseType.GetString()?.Contains("Belt") == true)
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
            Console.WriteLine($"[ERREUR] PoeNinja Belt : {ex.Message}");
        }

        return 0;
    }

    public async Task<List<BeltInfo>> GetAllBeltsAsync()
    {
        var beltList = new List<BeltInfo>();
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/itemoverview?league={league}&type=UniqueAccessory";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);
            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                if (item.TryGetProperty("baseType", out var baseType) && baseType.GetString()?.Contains("Belt") == true)
                {
                    var name = item.GetProperty("name").GetString() ?? "Unknown";
                    var price = item.TryGetProperty("chaosValue", out var cv) ? cv.GetDouble() : 0;

                    if (price > 0)
                    {
                        beltList.Add(new BeltInfo
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
            Console.WriteLine($"[ERREUR] GetAllBelts : {ex.Message}");
        }

        return beltList.OrderByDescending(b => b.ChaosValue).ToList();
    }
}