namespace BotPoe.Services;

public interface IPoePriceService
{
    Task<double?> GetPriceAsync(string currencyName);
}