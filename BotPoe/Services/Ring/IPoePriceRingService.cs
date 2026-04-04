using BotPoe.Models;

namespace BotPoe.Services.Ring;

public interface IPoePriceRingService
{
    Task<double> GetPriceRingAsync(string ringName);
    Task<List<RingInfo>> GetAllRingsAsync(); 
}