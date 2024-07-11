using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Gear : BattleEntity
{
    
    public Guid? CharacterGearEquipperId { get; set; }


    public Stat Stat { get; set; }



}