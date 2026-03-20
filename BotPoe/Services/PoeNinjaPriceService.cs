using System.Text.Json;

namespace BotPoe.Services;

public class PoeNinjaPriceService : IPoePriceService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoeNinjaPriceService(HttpClient http, ILeagueService leagueService)
    {
        _http = http;
        _leagueService = leagueService;
    }

    public async Task<double?> GetPriceAsync(string currencyName)
    {
        try
        {
            string league = await _leagueService.GetCurrentLeagueAsync();
            var url = $"https://poe.ninja/api/data/currencyoverview?league={league}&type=Currency";

            var response = await _http.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);

            var lines = document.RootElement.GetProperty("lines");

            foreach (var item in lines.EnumerateArray())
            {
                if (item.GetProperty("currencyTypeName").GetString() == currencyName)
                {
                    return item.GetProperty("chaosEquivalent").GetDouble();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] poe.ninja API : {ex.Message}");
        }
        return null;
    }
}