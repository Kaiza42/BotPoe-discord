namespace BotPoe.Services.Currency;

public interface IPoePriceCurrencyService
{
    Task<double?> GetPriceAsync(string currencyName);
}