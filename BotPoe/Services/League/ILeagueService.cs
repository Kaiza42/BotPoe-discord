namespace BotPoe.Services.League;

public interface ILeagueService
{
    Task<string> GetCurrentLeagueAsync();
}