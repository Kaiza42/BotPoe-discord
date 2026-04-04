using BotPoe.Models;
using BotPoe.Data;
using Microsoft.EntityFrameworkCore;

namespace BotPoe.Services.Regex;

public class RegexService
{
    public readonly Dictionary<string, (string Name, string Pattern)> Filters = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- SURVIE ---
        { "rfctm", ("Reflect Phys", "!f ph") },
        { "rfcte", ("Reflect Elem", "!eche") },
        { "cntrgn", ("No Regen", "!reg") },
        { "cntlch", ("No Leech", "!% of e") },
        { "avoid", ("Avoid Ailments", "!voi") },
        { "maxres", ("-Max Res", "!max.*res") },
        { "aura", ("Less Aura", "!auras") },
        { "curse", ("No Curses", "!curse") },
        { "speed", ("Monster Speed", "!mobs.*spe") },
        { "crit", ("Crit Multi", "!mult") },
        { "proj", ("Extra Proj", "!proj") },
        { "recov", ("Less Recovery", "!recov") },

        // --- QUANTITÉ & PACKSIZE ---
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

        // --- EXPEDITION ---
        { "dannig", ("Dannig", "dannig") },
        { "tujen", ("Tujen", "tujen") },
        { "rog", ("Rog", "rog") },
        { "gwen", ("Gwennen", "gwen") },
        { "radi", ("Explo Radius", "radi") },
        { "qexp", ("Exped Quant", "remn.*quant") },

        // --- FLACONS ---
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

    public async Task SaveToHistoryAsync(ulong userId, string name, string regex, string codes)
    {
        using var db = new AppDbContext();
        
        var existing = await db.RegexHistories
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Name == name);

        if (existing != null)
        {
            existing.RegexString = regex;
            existing.CodesUsed = codes;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            await db.RegexHistories.AddAsync(new RegexHistory 
            { 
                UserId = userId, 
                Name = name, 
                RegexString = regex, 
                CodesUsed = codes, 
                CreatedAt = DateTime.UtcNow 
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<RegexHistory>> GetUserHistoryAsync(ulong userId)
    {
        using var db = new AppDbContext();
        
        return await db.RegexHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(5)
            .ToListAsync();
    }
}