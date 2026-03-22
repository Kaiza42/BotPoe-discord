namespace BotPoe.Services;

public interface IPoePriceCurrencyService
{
    Task<double?> GetPriceAsync(string currencyName);
}