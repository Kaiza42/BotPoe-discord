using Discord.Commands;
using Discord.WebSocket;
using BotPoe.Services.Ring; 
using BotPoe.Services.Currency;
using Discord;

namespace BotPoe.Services.Message;

public class PriceRingMonitorService : ModuleBase<SocketCommandContext>
{
    private readonly IPoePriceRingService _ringService;
    private readonly IPoePriceCurrencyService _currencyService;

    public PriceRingMonitorService(IPoePriceRingService ringService, IPoePriceCurrencyService currencyService)
    {
        _ringService = ringService;
        _currencyService = currencyService;
    }

    [Command("ring")]
    [Summary("Demande le prix d'un anneau spécifique")]
    public async Task AskAndGetRingPrice()
    {
        await ReplyAsync("💍 **Pour quel anneau souhaites-tu connaître le prix ?**");

        var response = await GetNextMessageAsync(TimeSpan.FromSeconds(30));

        if (response != null)
        {
            string ringName = response.Content;
            double priceChaos = await _ringService.GetPriceRingAsync(ringName);

            if (priceChaos > 0)
            {
                string messageFinal = $"✅ Le prix pour **{ringName}** est de : `{priceChaos}c`";
                
                if (priceChaos >= 500)
                {
                    double? divinePrice = await _currencyService.GetPriceAsync("Divine Orb");

                    if (divinePrice.HasValue && divinePrice.Value > 0)
                    {
                        double priceInDiv = priceChaos / divinePrice.Value;
                        messageFinal += $" (soit environ **{Math.Round(priceInDiv, 1)} Divines**)";
                    }
                }

                await ReplyAsync(messageFinal);
            }
            else
            {
                await ReplyAsync($"❌ Impossible de trouver l'anneau `{ringName}`. Vérifie l'orthographe !");
            }
        }
    }

    [Command("rings")]
    [Alias("AllRings")]
    [Summary("Affiche la liste des anneaux les plus chers")]
    public async Task ShowAllRings()
    {
        var loadingMsg = await ReplyAsync("🔍 Analyse du marché des anneaux uniques...");
        
        List<BotPoe.Models.RingInfo> allRings = await _ringService.GetAllRingsAsync();
        double? divPrice = await _currencyService.GetPriceAsync("Divine Orb");

        if (allRings != null && allRings.Any())
        {
            var embed = new EmbedBuilder()
                .WithTitle("💎 Liste des Anneaux Uniques")
                .WithColor(Color.Blue) // Couleur différente pour différencier des ceintures
                .WithThumbnailUrl("https://web.poecdn.com/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvUmluZ3MvS2FsYW5kcmFzVG91Y2giLCJzYyI6MX1d/0312019808/KalandrasTouch.png")
                .WithFooter(footer => footer.Text = "Prix basés sur Poe.Ninja")
                .WithCurrentTimestamp();
            
            var filteredRings = allRings.Where(r => r.ChaosValue >= 5).Take(40).ToList();

            string namesColumn = "";
            string pricesColumn = "";

            foreach (var ring in filteredRings)
            {
                string nextName = $"• {ring.Name}\n";
                string p = $"{Math.Round(ring.ChaosValue, 0)}c";
                
                if (ring.ChaosValue >= 500 && divPrice > 0)
                {
                    p += $" ({Math.Round(ring.ChaosValue / divPrice.Value, 1)}d)";
                }
                string nextPrice = $"{p}\n";
                
                if (namesColumn.Length + nextName.Length > 1000 || pricesColumn.Length + nextPrice.Length > 1000)
                    break;

                namesColumn += nextName;
                pricesColumn += nextPrice;
            }

            embed.AddField("💍 Anneau", namesColumn, inline: true);
            embed.AddField("💰 Prix", pricesColumn, inline: true);

            await loadingMsg.DeleteAsync();
            await ReplyAsync(embed: embed.Build());
        }
        else
        {
            await loadingMsg.ModifyAsync(msg => msg.Content = "❌ Impossible de récupérer les prix des anneaux.");
        }
    }
    
    private async Task<SocketUserMessage?> GetNextMessageAsync(TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<SocketUserMessage?>();

        async Task Handler(SocketMessage s)
        {
            if (s is not SocketUserMessage msg || s.Author.Id != Context.User.Id || s.Channel.Id != Context.Channel.Id)
                return;

            tcs.TrySetResult(msg);
            await Task.CompletedTask;
        }

        Context.Client.MessageReceived += Handler;
        var result = await Task.WhenAny(tcs.Task, Task.Delay(timeout));
        Context.Client.MessageReceived -= Handler;

        return result == tcs.Task ? await tcs.Task : null;
    }
}