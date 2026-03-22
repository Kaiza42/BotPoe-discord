namespace BotPoe.Services;

public interface IPoePriceBeltService
{
    Task<double> GetPriceBeltAsync(string BeltName);
}