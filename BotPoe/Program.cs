using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using BotPoe.Services.belt;
using BotPoe.Services.Currency;
using BotPoe.Services.Beast;
using BotPoe.Services.Essence;
using BotPoe.Services.Message;
using BotPoe.Models;
using BotPoe.Services.League;
using BotPoe.Services.Ring;
using BotPoe.Services.Regex;
using BotPoe.Data;
using DotNetEnv;

namespace BotPoe;

class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        
        using (var db = new AppDbContext())
        {
            await db.Database.EnsureCreatedAsync();
        }

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
                    LogLevel = LogSeverity.Error 
                }));
                services.AddSingleton<BotStateService>();
                services.AddSingleton<CommandService>();
                services.AddSingleton<ILeagueService, GggLeagueService>();
                services.AddSingleton<IPoePriceCurrencyService, PoePriceCurrencyService>();
                services.AddSingleton<IPoePriceBeltService, PoePriceBeltService>();
                services.AddSingleton<IPoePriceBeastService, PoePriceBeastService>();
                services.AddSingleton<IPoePriceEssenceService, PoePriceEssenceService>();
                services.AddHostedService<PriceMonitorService>();
                services.AddSingleton<AdminModule>();
                services.AddSingleton<IPoePriceRingService, PoePriceRingService>();
                services.AddSingleton<RegexModule>();
                services.AddSingleton<RegexService>();
            })
            .Build();

        var client = host.Services.GetRequiredService<DiscordSocketClient>();
        var commands = host.Services.GetRequiredService<CommandService>();
        var servicesProvider = host.Services;

        client.MessageReceived += (msg) =>
        {
            _ = Task.Run(async () =>
            {
                if (msg is not SocketUserMessage message || message.Author.IsBot) return;

                int argPos = 0;
                if (message.HasStringPrefix("!", ref argPos))
                {
                    var context = new SocketCommandContext(client, message);
                    await commands.ExecuteAsync(context, argPos, servicesProvider);
                }
            });

            return Task.CompletedTask;
        };

        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), servicesProvider);
        
        string? token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? config["BotToken"];
        
        if (string.IsNullOrEmpty(token)) return;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await host.RunAsync();
    }
}