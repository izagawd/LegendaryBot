
namespace DiscordBotNet.Database.Models;

public class GuildData
{
    public ulong Id { get; set; }

    public List<string> EnabledCommands { get; set; } = [];

}
