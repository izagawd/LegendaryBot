namespace DiscordBotNet.Database.ManyToManyInstances;

public class CharacterPlayerTeam
{
    public Guid CharacterId { get; set; }
    
    public Guid PlayerTeamId { get; set; }
}