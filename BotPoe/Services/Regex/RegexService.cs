using BotPoe.Models;

namespace BotPoe.Services.Regex;

public class RegexService
{

public readonly Dictionary<string, (string Name, string Pattern)> Filters = new(StringComparer.OrdinalIgnoreCase)
{
    // --- SURVIE : MODS LÉTAUX (Format "Not" : !) ---
    { "rfctm", ("Reflect Phys", "!f ph") },      // "Reflects #% of Physical"
    { "rfcte", ("Reflect Elem", "!eche") },     // "Reflects Elemental"
    { "cntrgn", ("No Regen", "!reg") },        // "Regenerate"
    { "cntlch", ("No Leech", "!% of e") },     // "Life % of Energy Shield" (Leech)
    { "avoid", ("Avoid Ailments", "!voi") },    // "Avoid Status"
    { "maxres", ("-Max Res", "!max.*res") },   // "Maximum resistance"
    { "aura", ("Less Aura", "!auras") },       // "Auras"
    { "curse", ("No Curses", "!curse") },      // "Curses"
    { "speed", ("Monster Speed", "!mobs.*spe") }, 
    { "crit", ("Crit Multi", "!mult") },       // "Multiplier"
    { "proj", ("Extra Proj", "!proj") },       // "Projectiles"
    { "recov", ("Less Recovery", "!recov") },   // "Recovery"

    // --- RENTABILITÉ : QUANTITÉ & PACKSIZE ---
    { "q80", ("80%+ Quant", "quant.*[8-9][0-9]%") },
    { "q90", ("90%+ Quant", "quant.*[9][0-9]%") },
    { "q100", ("100%+ Quant", "quant.*1[0-9][0-9]%") },
    { "p30", ("30%+ Pack Size", "size.*[3-9][0-9]%") },
    { "p40", ("40%+ Pack Size", "size.*[4-9][0-9]%") },

    // --- CONTENU & ATLAS ---
    { "beyond", ("Beyond", "beyo") },
    { "deli", ("Delirium", "deli") },
    { "exar", ("Exarch", "exar") },
    { "eate", ("Eater", "eate") },
    { "conq", ("Conqueror", "conq") },
    { "guar", ("Guardian", "guar") },

    // --- EXPEDITION (LOGBOOKS) ---
    { "dannig", ("Dannig", "dannig") },
    { "tujen", ("Tujen", "tujen") },
    { "rog", ("Rog", "rog") },
    { "gwen", ("Gwennen", "gwen") },
    { "radi", ("Explo Radius", "radi") },
    { "qexp", ("Exped Quant", "remn.*quant") },

    // --- FLACONS (FLASKS) ---
    { "full", ("Use when Full", "full") },
    { "hit", ("Charge on Hit", "gai.*hit") },
    { "critg", ("Charge on Crit", "gai.*cri") },
    { "dur", ("Duration", "dur") },

    // --- DIVERS ---
    { "corr", ("Corrupted", "corr") },
    { "unid", ("Unidentified", "unid") },
    { "qual", ("20% Quality", "ual.*20") },
    { "white", ("White Item", "^$") }
};

    private readonly List<RegexHistory> _history = new();

    public void SaveToHistory(ulong userId, string name, string regex, string codes)
    {
        _history.Add(new RegexHistory 
        { 
            UserId = userId, 
            Name = name,
            RegexString = regex, 
            CodesUsed = codes,
            CreatedAt = DateTime.Now 
        });

        // Sécurité pour ne pas saturer la RAM (limite à 100 entrées totales)
        if (_history.Count > 100) _history.RemoveAt(0);
    }

    public List<RegexHistory> GetUserHistory(ulong userId)
    {
        return _history.Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(5) // On retourne les 5 derniers
            .ToList();
    }
}