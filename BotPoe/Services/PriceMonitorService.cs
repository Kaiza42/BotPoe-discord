using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BotPoe.Services;

public class PriceMonitorService : BackgroundService
{
    private readonly IPoePriceCurrencyService _priceService;
    private readonly DiscordSocketClient _client;
    private readonly ulong _channelId;
    private double? _previousPrice;
    private DateTime _lastDailyPostDate = DateTime.MinValue;

    public PriceMonitorService(IPoePriceCurrencyService priceService, DiscordSocketClient client, IConfiguration config)
    {
        _priceService = priceService;
        _client = client;
        _channelId = ulong.Parse(config["PriceAlertChannelId"] ?? "0");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_client.ConnectionState != ConnectionState.Connected)
            await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var currentPrice = await _priceService.GetPriceAsync("Divine Orb");

            if (currentPrice.HasValue)
            {
                if (_previousPrice.HasValue)
                {
                    double difference = currentPrice.Value - _previousPrice.Value;

                    if (difference >= 20)
                    {
                        await SendAlertAsync($"🚀 **ALERTE ÉCONOMIE** : Le prix de la Divine a bondi !\n" +
                                           $"• Il y a 30 min : `{_previousPrice}c`\n" +
                                           $"• Maintenant : **{currentPrice}c**\n" +
                                           $"📈 Augmentation de **+{difference} Chaos**");
                    }
                }

                _previousPrice = currentPrice;

                if (now.Hour == 18 && _lastDailyPostDate.Date != now.Date)
                {
                    await SendAlertAsync($"📅 **Rapport Quotidien (18h00)**\n" +
                                       $"Le prix moyen de la Divine Orb est de **{currentPrice} Chaos**.");
                    _lastDailyPostDate = now;
                }
            }
            await SendAlertAsync("Le bot POE est lancé !");
            await SendAlertAsync($"Le prix d'achat de la divine est : **{currentPrice} Chaos**");
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }

    }

    private async Task SendAlertAsync(string message)
    {
        try
        {
            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            if (channel != null)
            {
                await channel.SendMessageAsync(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR] Impossible d'envoyer le message : {ex.Message}");
        }
    }
}