using BotPoe.Models;

namespace BotPoe.Services.belt;

public interface IPoePriceBeltService
{
    Task<double> GetPriceBeltAsync(string BeltName);
    Task<List<BeltInfo>> GetAllBeltsAsync();
}