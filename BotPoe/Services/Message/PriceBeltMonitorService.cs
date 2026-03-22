using Discord.Commands;
using Discord.WebSocket;
using BotPoe.Services.belt;

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