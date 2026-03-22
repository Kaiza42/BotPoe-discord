
using BotPoe.Models;

namespace BotPoe.Services.Beast;

public interface IPoePriceBeastService
{
    Task<List<BeastInfo>> GetAllBeastsAsync();
}