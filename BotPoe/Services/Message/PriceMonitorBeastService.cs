using BotPoe.Services.Beast;
using BotPoe.Services.Currency;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BotPoe.Services.Message;

public class PriceMonitorBeastService : ModuleBase<SocketCommandContext>
{
    private readonly IPoePriceBeastService _beastService;
    private readonly IPoePriceCurrencyService _currencyService;

    public PriceMonitorBeastService(IPoePriceBeastService beastService, IPoePriceCurrencyService currencyService)
    {
        _beastService = beastService;
        _currencyService = currencyService;
    }

    [Command("beasts")]
    public async Task ShowAllBeasts()
    {
        var loadingMsg = await ReplyAsync("🔍 Récupération du bestiaire complet... Veuillez patienter.");

        var allBeasts = await _beastService.GetAllBeastsAsync();
        double? divPrice = await _currencyService.GetPriceAsync("Divine Orb");

        if (allBeasts != null && allBeasts.Any())
        {
            var embed = new EmbedBuilder()
                .WithTitle("📜 Bestiaire de la Ménagerie (Prix Actuels)")
                .WithColor(Color.DarkGreen)
                .WithThumbnailUrl("https://web.poecdn.com/image/beast-hunter/beast-tab-icon.png")
                .WithFooter(footer => footer.Text = $"Basé sur la ligue actuelle • Divine : {divPrice ?? 0}c")
                .WithCurrentTimestamp();

            string namesColumn = "";
            string pricesColumn = "";

            var topBeasts = allBeasts
                .OrderByDescending(b => b.ChaosValue)
                .Take(20);

            foreach (var beast in topBeasts)
            {
                namesColumn += $"• {beast.Name}\n";

                string p = $"{Math.Round(beast.ChaosValue, 0)}c";
                if (beast.ChaosValue >= 500 && divPrice > 0)
                {
                    p += $" ({Math.Round(beast.ChaosValue / divPrice.Value, 1)}d)";
                }
                pricesColumn += $"{p}\n";
            }

            embed.AddField("🐾 Bête", namesColumn, inline: true);
            embed.AddField("💰 Prix", pricesColumn, inline: true);

            await loadingMsg.DeleteAsync();
            await ReplyAsync(embed: embed.Build());
        }
        else
        {
            await loadingMsg.ModifyAsync(msg => msg.Content = "❌ Impossible de charger les données de PoE Ninja.");
        }
    }
}