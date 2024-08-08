using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class CharacterStats
{
    [NotMapped] public float TotalAttack;
    [NotMapped] public float TotalDefense;
    [NotMapped] public float TotalMaxHealth;
    [NotMapped] public float TotalSpeed;
    [NotMapped] public float TotalCriticalChance;
    [NotMapped] public float TotalCriticalDamage;
    [NotMapped] public float TotalEffectiveness;
    [NotMapped] public float TotalResistance;
}