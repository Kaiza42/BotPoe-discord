namespace BotPoe.Services;

public interface ILeagueService
{
    Task<string> GetCurrentLeagueAsync();
}