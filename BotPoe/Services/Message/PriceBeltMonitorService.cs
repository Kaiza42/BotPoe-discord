using Discord.Commands;
using Discord.WebSocket;
using BotPoe.Services.belt;
using BotPoe.Services.Currency;
using Discord;

namespace BotPoe.Services.Currency.Message;

// The goal of this page is to ask the user for a belt name and return the price of the requested belt.
public class PriceBeltMonitorService : ModuleBase<SocketCommandContext>
{
    private readonly IPoePriceBeltService _beltService;
    private readonly IPoePriceCurrencyService _currencyService;

    public PriceBeltMonitorService(IPoePriceBeltService beltService, IPoePriceCurrencyService currencyService)
    {
        _beltService = beltService;
        _currencyService = currencyService;
    }

    [Command("belt")]
    public async Task AskAndGetBeltPrice()
    {
        await ReplyAsync("👉 **Pour quelle ceinture souhaites-tu connaître le prix ?**");

        var response = await GetNextMessageAsync(TimeSpan.FromSeconds(30));

        if (response != null)
        {
            string beltName = response.Content;
            double priceChaos = await _beltService.GetPriceBeltAsync(beltName);

            if (priceChaos > 0)
            {
                string messageFinal = $"✅ Le prix pour **{beltName}** est de : `{priceChaos}c`";

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
                await ReplyAsync($"❌ Impossible de trouver `{beltName}`.");
            }
        }
    }

[Command("belts")]
[Alias("AllBelt")]
public async Task ShowAllBelts()
{
    var loadingMsg = await ReplyAsync("🔍 Analyse du marché des ceintures uniques...");

    var allBelts = await _beltService.GetAllBeltsAsync();
    double? divPrice = await _currencyService.GetPriceAsync("Divine Orb");

    if (allBelts != null && allBelts.Any())
    {
        var embed = new EmbedBuilder()
            .WithTitle("💎 Liste des Ceintures Uniques")
            .WithColor(Color.Gold)
            .WithThumbnailUrl("https://web.poecdn.com/gen/image/WzI1LDE0LHsiZiI6IjJESXRlbXMvQmVsdHMvTWFnZWJsb29kIiwic2MiOjF9XQ/05f3248356/Mageblood.png")
            .WithFooter(footer => footer.Text = "Variantes incluses • Prix mis à jour")
            .WithCurrentTimestamp();

        var filteredBelts = allBelts.Where(b => b.ChaosValue >= 2).Take(40).ToList();

        string namesColumn = "";
        string pricesColumn = "";

        foreach (var belt in filteredBelts)
        {
            string nextName = $"• {belt.Name}\n";
            string p = $"{Math.Round(belt.ChaosValue, 0)}c";
            if (belt.ChaosValue >= 500 && divPrice > 0)
            {
                p += $" ({Math.Round(belt.ChaosValue / divPrice.Value, 0)}d)";
            }
            string nextPrice = $"{p}\n";

            if (namesColumn.Length + nextName.Length > 1000 || pricesColumn.Length + nextPrice.Length > 1000)
                break;

            namesColumn += nextName;
            pricesColumn += nextPrice;
        }

        embed.AddField("🎗️ Ceinture", namesColumn, inline: true);
        embed.AddField("💰 Prix", pricesColumn, inline: true);

        await loadingMsg.DeleteAsync();
        await ReplyAsync(embed: embed.Build());
    }
    else
    {
        await loadingMsg.ModifyAsync(msg => msg.Content = "❌ Impossible de récupérer les prix.");
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