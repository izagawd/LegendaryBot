namespace DiscordBotNet.Database.ManyToManyInstances;

public class CharacterPlayerTeam
{
    public long CharacterId { get; set; }
    
    public long PlayerTeamId { get; set; }
}