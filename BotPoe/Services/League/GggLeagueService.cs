using System.Text.Json;

namespace BotPoe.Services;

// This page is only used to look up the name of the current POE league via the GGG API.
public class GggLeagueService : ILeagueService
{
    private readonly HttpClient _http;
    private string? _cachedLeague;
    private DateTime _lastUpdate = DateTime.MinValue;

    public GggLeagueService(HttpClient http)
    {
        _http = http;
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) BotPoe/1.0");
        }
    }

    public async Task<string> GetCurrentLeagueAsync()
    {
        if (_cachedLeague != null && (DateTime.UtcNow - _lastUpdate).TotalHours < 24)
            return _cachedLeague;

        try
        {
            var url = "https://www.pathofexile.com/api/trade/data/leagues";
            var response = await _http.GetStringAsync(url);

            using var document = JsonDocument.Parse(response);

            var result = document.RootElement.GetProperty("result");

            foreach (var league in result.EnumerateArray())
            {
                var id = league.GetProperty("id").GetString();

                if (id != null &&
                    !id.Contains("Standard") &&
                    !id.Contains("Hardcore") &&
                    !id.Contains("SSF") &&
                    !id.Contains("Ruthless"))
                {
                    _cachedLeague = id;
                    _lastUpdate = DateTime.UtcNow;
                    Console.WriteLine($"[LOG] Ligue détectée via Trade API : {_cachedLeague}");
                    return _cachedLeague;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] Échec de la récupération des ligues : {ex.Message}");
        }

        return "Standard";
    }
}