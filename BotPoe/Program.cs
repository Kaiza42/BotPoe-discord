using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotPoe;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        using var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                LogLevel = LogSeverity.Info
            }))
            .BuildServiceProvider();

        var client = services.GetRequiredService<DiscordSocketClient>();

        client.Log += (msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        string? token = config["BotToken"];

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("ERREUR : Le Token est vide ou introuvable dans appsettings.json !");
            return;
        }

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        client.MessageReceived += async (msg) =>
        {
            if (msg.Author.IsBot) return;
            if (msg.Content.ToLower() == "!test")
                await msg.Channel.SendMessageAsync("✅ Bot connecté avec succès via appsettings.json !");
        };

        await Task.Delay(-1);
    }
}