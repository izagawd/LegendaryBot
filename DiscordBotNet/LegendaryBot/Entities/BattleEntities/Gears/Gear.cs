using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public class Gear : BattleEntity
{
    
    public Guid? CharacterGearEquipperId { get; set; }


    public GearStat MainStat { get; set; } 

    public List<GearStat> Stats { get; set; } = [];

    
}