using BotPoe.Models;

namespace BotPoe.Services.Essence;

public interface IPoePriceEssenceService
{
    Task<List<EssenceInfo>> GetAllEssencesAsync();
}