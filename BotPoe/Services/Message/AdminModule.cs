using BotPoe.Models;
using Discord.Commands;

namespace BotPoe.Services.Message;
// The purpose of this page is simply to turn off the bot so it stops sending alerts
public class AdminModule : ModuleBase<SocketCommandContext>
{
    private readonly BotStateService _botState;
    
    public AdminModule(BotStateService botState)
    {
        _botState = botState;
    }

    [Command("Start")]
    [Summary("Permet d'activé le bot")]
    public async Task StartBot()
    {
        if (_botState.IsEnabled)
        {
            await ReplyAsync("Le bot est déja activé");
            return;
        }
        
        _botState.IsEnabled = true;
        await ReplyAsync("Le bot est activé ! ");
    }

    [Command("Stop")]
    [Summary("Permet de désactivé le bot")]
    public async Task StopBot()
    {
        if (!_botState.IsEnabled)
        {
            await ReplyAsync("Le bot est déja off");
            return;
        }
        _botState.IsEnabled = false;
        await ReplyAsync("Le bot est off");
    }

    [Command("Status")]
    [Summary("Permet de verifier le status du bot")]
    public async Task GetStatus()
    {
        string etat = _botState.IsEnabled ? "ALLUMÉ (ON) ✅" : "ÉTEINT (OFF) 🛑";
        await ReplyAsync($"📊 État actuel du bot : **{etat}**");
    }
}