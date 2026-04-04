namespace BotPoe.Models;

public class RegexHistory
{
    public ulong UserId { get; set; }
    public string Name { get; set; } = "NoName"; 
    public string RegexString { get; set; } = string.Empty;
    public string CodesUsed { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}