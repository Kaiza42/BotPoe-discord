using System.Text.Json;
using BotPoe.Services.League;


namespace BotPoe.Services.Currency;

// This page is used to look up the price of, for example, the Divine in Chaos on PoeNinja.
// The price is the chaos equivalent, not the price listed on PoE Ninja.
public class PoePriceCurrencyService : IPoePriceCurrencyService
{
    private readonly HttpClient _http;
    private readonly ILeagueService _leagueService;

    public PoePriceCurrencyService(HttpClient http, ILeagueService leagueService)
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
                    if (item.TryGetProperty("chaosEquivalent", out var chaosElement))
                    {
                        return Math.Round(chaosElement.GetDouble(), 1);
                    }

                    if (item.TryGetProperty("receive", out var receiveElement))
                    {
                        double price = receiveElement.GetProperty("value").GetDouble();
                        return Math.Round(price, 1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] PoeNinja : {ex.Message}");
        }
        return null;
    }
}
