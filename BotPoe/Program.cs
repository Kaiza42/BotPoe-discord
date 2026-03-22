using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using BotPoe.Services;
using BotPoe.Services.belt;
using BotPoe.Services.Currency;
using BotPoe.Services.Beast;
using BotPoe.Services.Essence;
using BotPoe.Services.Message;

namespace BotPoe;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {

                services.AddSingleton<IConfiguration>(config);
                services.AddSingleton<HttpClient>();
                services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Info
                }));

                services.AddSingleton<CommandService>();
                services.AddSingleton<ILeagueService, GggLeagueService>();
                services.AddSingleton<IPoePriceCurrencyService, PoePriceCurrencyService>();
                services.AddSingleton<IPoePriceBeltService, PoePriceBeltService>();
                services.AddSingleton<IPoePriceBeastService, PoePriceBeastService>();
                services.AddSingleton<IPoePriceEssenceService, PoePriceEssenceService>();
                services.AddHostedService<PriceMonitorService>();
            })
            .Build();

        var client = host.Services.GetRequiredService<DiscordSocketClient>();
        var commands = host.Services.GetRequiredService<CommandService>();
        var services = host.Services;

        client.Log += (msg) => { Console.WriteLine(msg.ToString()); return Task.CompletedTask; };

        client.MessageReceived += (msg) =>
        {
            _ = Task.Run(async () =>
            {
                if (msg is not SocketUserMessage message || message.Author.IsBot) return;

                int argPos = 0;
                if (message.HasStringPrefix("!", ref argPos))
                {
                    var context = new SocketCommandContext(client, message);
                    var result = await commands.ExecuteAsync(context, argPos, services);

                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine($"[ERREUR COMMANDE] {result.ErrorReason}");
                    }
                }
            });

            return Task.CompletedTask;
        };

        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

        string? token = config["BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[ERREUR] Le token du bot est introuvable dans appsettings.json.");
            return;
        }

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        Console.WriteLine("[SYSTEM] Bot prêt ! Commandes : !belt, !beast");

        await host.RunAsync();
    }
}