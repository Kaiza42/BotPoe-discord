namespace BotPoe.Services.belt;

public interface IPoePriceBeltService
{
    Task<double> GetPriceBeltAsync(string BeltName);
}