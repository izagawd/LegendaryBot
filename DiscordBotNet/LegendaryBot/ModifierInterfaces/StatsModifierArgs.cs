using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.ModifierInterfaces;

public abstract class StatsModifierArgs 
{
    /// <summary>
    /// The value to either increase or decrease with
    /// </summary>
    public float ValueToChangeWith { get; init; }

    public required Character CharacterToAffect { get; init; }
 

    
}