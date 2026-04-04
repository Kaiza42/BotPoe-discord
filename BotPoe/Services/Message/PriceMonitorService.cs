using BotPoe.Services.Currency;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using BotPoe.Models;

namespace BotPoe.Services.Message;

public class PriceMonitorService : BackgroundService
{
    private readonly IPoePriceCurrencyService _priceService;
    private readonly DiscordSocketClient _client;
    private readonly BotStateService _botState; 
    private readonly ulong _channelId;
    private double? _previousPrice;
    private DateTime _lastDailyPostDate = DateTime.MinValue;

    public PriceMonitorService(
        IPoePriceCurrencyService priceService, 
        DiscordSocketClient client, 
        IConfiguration config,
        BotStateService botState)
    {
        _priceService = priceService;
        _client = client;
        _botState = botState;
        _channelId = ulong.Parse(config["PriceAlertChannelId"] ?? "0");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (_client.ConnectionState != ConnectionState.Connected)
            await Task.Delay(1000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_botState.IsEnabled)
            {
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            var now = DateTime.Now;
            var currentPrice = await _priceService.GetPriceAsync("Divine Orb");

            if (currentPrice.HasValue)
            {
                if (_previousPrice.HasValue)
                {
                    double difference = currentPrice.Value - _previousPrice.Value;

                    if (Math.Abs(difference) >= 20) 
                    {
                        string emoji = difference > 0 ? "🚀" : "📉";
                        await SendAlertAsync($"{emoji} **ALERTE ÉCONOMIE** : Le prix de la Divine a bougé !\n" +
                                           $"• Il y a 30 min : `{_previousPrice}c`\n" +
                                           $"• Maintenant : **{currentPrice}c**\n" +
                                           $"• Variation : **{difference} Chaos**");
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
            
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    /// <summary>
    /// Envoie un message uniquement si le bot est sur ON
    /// </summary>
    private async Task SendAlertAsync(string message)
    {
        if (!_botState.IsEnabled) return; 

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
            Console.WriteLine($"[ERREUR] Envoi impossible : {ex.Message}");
        }
    }
}