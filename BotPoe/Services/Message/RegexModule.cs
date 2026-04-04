using Discord.Commands;
using Discord;
using BotPoe.Services.Regex;

namespace BotPoe.Services.Message;

public class RegexModule : ModuleBase<SocketCommandContext>
{
    private readonly RegexService _regexService;

    public RegexModule(RegexService regexService)
    {
        _regexService = regexService;
    }

    [Command("rgxm")]
    [Summary("Affiche le dictionnaire des codes de maniere claire")]
    public async Task ShowRegexCodes()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Dictionnaire des Codes Regex")
            .WithDescription("Syntaxe : !rgx [Nom] [Codes...]\nExemple : !rgx Filtre1 rfctm q100")
            .WithColor(Color.DarkBlue);
        
        embed.AddField("1. SURVIE", 
            "```\n" +
            "rfctm  : Reflect Physique\n" +
            "rfcte  : Reflect Elementaire\n" +
            "cntrgn : Pas de Regeneration\n" +
            "cntlch : Pas de Leech\n" +
            "avoid  : Eviter Alterations\n" +
            "maxres : - Max Resistance\n" +
            "aura   : Moins d'effet d'Aura\n" +
            "curse  : Maledictions (Curses)\n" +
            "speed  : Vitesse des Monstres\n" +
            "crit   : Degats des Crits\n" +
            "proj   : Projectiles en plus\n" +
            "recov  : Moins de Recuperation\n" +
            "```");
        
        embed.AddField("2. RENTABILITE", 
            "```\n" +
            "q80/q90/q100 : Quantite de Map\n" +
            "p30/p40      : Taille de Groupes\n" +
            "beyond       : Beyond\n" +
            "deli         : Delirium\n" +
            "exar/eate    : Influence Exarch/Eater\n" +
            "conq/guar    : Map Conquerant/Gardien\n" +
            "```");
        
        embed.AddField("3. EXPEDITION ", 
            "```\n" +
            "dannig : Logbook Dannig\n" +
            "tujen  : Logbook Tujen\n" +
            "rog    : Logbook Rog\n" +
            "gwen   : Logbook Gwennen\n" +
            "radi   : Rayon d'Explosion\n" +
            "qexp   : Quantite Expedition\n" +
            "```");

        embed.AddField("4. DIVERS", 
            "```\n" +
            "full   : Flacon Plein (Auto)\n" +
            "hit    : Charge Flacon par Coup\n" +
            "critg  : Charge Flacon par Crit\n" +
            "dur    : Duree des Flacons\n" +
            "corr   : Corrompu\n" +
            "unid   : Non-Identifie\n" +
            "qual   : Qualite 20%\n" +
            "white  : Item Blanc (Base)\n" +
            "```");

        embed.WithFooter("Rappel : Additionnez les codes avec un espace. Exemple : !rgx MaMap rfctm q100");

        await ReplyAsync(embed: embed.Build());
    }

    [Command("rgx")]
    [Summary("Genere et enregistre un regex")]
    public async Task BuildRegex(string name, params string[] codes)
    {
        if (codes.Length == 0)
        {
            await ReplyAsync("Erreur : Precise des codes apres le nom. Exemple : !rgx Filtre1 rfctm q100");
            return;
        }

        var patterns = new List<string>();
        var foundCodes = new List<string>();

        foreach (var code in codes)
        {
            if (_regexService.Filters.TryGetValue(code, out var data))
            {
                patterns.Add(data.Pattern);
                foundCodes.Add(code);
            }
        }

        if (patterns.Any())
        {
            string finalRegex = $"\"{string.Join("|", patterns)}\"";
            
            // APPEL ASYNC VERS POSTGRESQL
            await _regexService.SaveToHistoryAsync(Context.User.Id, name, finalRegex, string.Join(", ", foundCodes));

            var embed = new EmbedBuilder()
                .WithTitle($"Filtre enregistre : {name}")
                .WithColor(Color.Green)
                .AddField("Texte a copier dans le jeu", $"```\n{finalRegex}\n```")
                .WithFooter($"Combinaison : {string.Join(" + ", foundCodes)}");

            await ReplyAsync(embed: embed.Build());
        }
        else
        {
            await ReplyAsync("Erreur : Aucun code valide detecte. Tapez !rgxm pour la liste.");
        }
    }

    [Command("rgxHistory")]
    [Alias("rgxh")]
    public async Task ShowHistory()
    {
        var history = await _regexService.GetUserHistoryAsync(Context.User.Id);

        if (!history.Any())
        {
            await ReplyAsync("Historique vide.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Historique des filtres")
            .WithColor(Color.Gold);

        foreach (var item in history)
        {
            embed.AddField($"{item.Name} ({item.CreatedAt:dd/MM HH:mm})", 
                           $"Codes : {item.CodesUsed}\n```\n{item.RegexString}\n```");
        }

        await ReplyAsync(embed: embed.Build());
    }
}