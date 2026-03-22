using Discord;
using Discord.Commands;
using BotPoe.Services.Essence;
using BotPoe.Services.Currency;

namespace BotPoe.Services.Message;

public class PriceEssenceMonitorService : ModuleBase<SocketCommandContext>
{
    private readonly IPoePriceEssenceService _essenceService;
    private readonly IPoePriceCurrencyService _currencyService;

    public PriceEssenceMonitorService(IPoePriceEssenceService essenceService, IPoePriceCurrencyService currencyService)
    {
        _essenceService = essenceService;
        _currencyService = currencyService;
    }

    [Command("essence")]
    [Alias("essence")]
    public async Task ShowEssences()
    {
        var loadingMsg = await ReplyAsync("💎 Récupération des prix des essences...");

        var allEssences = await _essenceService.GetAllEssencesAsync();
        double? divPrice = await _currencyService.GetPriceAsync("Divine Orb");

        if (allEssences != null && allEssences.Any())
        {
            var embed = new EmbedBuilder()
                .WithTitle("💎 Prix des Essences")
                .WithColor(Color.Purple)
                .WithThumbnailUrl("https://web.poecdn.com/image/Art/2DItems/Currency/Essence/Horror.png")
                .WithCurrentTimestamp();

            string namesColumn = "";
            string pricesColumn = "";

            var topEssences = allEssences.Take(40).ToList();

            foreach (var essence in topEssences)
            {
                string nextName = $"• {essence.Name.Replace("Deafening Essence of ", "")}\n";
                string nextPrice = $"{Math.Round(essence.ChaosValue, 1)}c\n";

                if (namesColumn.Length + nextName.Length > 1000) break;

                namesColumn += nextName;
                pricesColumn += nextPrice;
            }

            embed.AddField("✨ Essence (Deafening)", namesColumn, inline: true);
            embed.AddField("💰 Prix", pricesColumn, inline: true);

            await loadingMsg.DeleteAsync();
            await ReplyAsync(embed: embed.Build());
        }
        else
        {
            await loadingMsg.ModifyAsync(msg => msg.Content = "❌ Impossible de charger les essences.");
        }
    }
}