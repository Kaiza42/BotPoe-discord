using Microsoft.Extensions.DependencyInjection;
using BotPoe.Services;

namespace BotPoe;

class Program
{
    public static async Task Main(string[] args)
    {
        // 1. On prépare le moteur avec nos deux services
        using var services = new ServiceCollection()
            .AddSingleton<HttpClient>()
            .AddSingleton<ILeagueService, GggLeagueService>()      // Brique 1
            .AddSingleton<IPoePriceService, PoeNinjaPriceService>() // Brique 2
            .BuildServiceProvider();

        Console.WriteLine("[TEST] Vérification de la chaîne complète...");

        // 2. On récupère le service de prix
        var priceService = services.GetRequiredService<IPoePriceService>();

        // 3. On demande le prix d'une Divine
        Console.WriteLine("Interrogation de poe.ninja pour la Divine Orb...");
        var price = await priceService.GetPriceAsync("Divine Orb");

        // 4. Résultat final
        Console.WriteLine("-----------------------------------------");
        if (price.HasValue)
        {
            Console.WriteLine($"✅ SUCCÈS : 1 Divine Orb vaut {price.Value} Chaos !");
        }
        else
        {
            Console.WriteLine("❌ ÉCHEC : Impossible de trouver le prix.");
        }
        Console.WriteLine("-----------------------------------------");
    }
}