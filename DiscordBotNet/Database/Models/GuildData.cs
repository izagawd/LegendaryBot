
namespace DiscordBotNet.Database.Models;

public class GuildData
{
    public long Id { get; set; }

    public List<string> EnabledCommands { get; set; } = [];

}
