using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;

public class SpeedPercentageGearStat : GearStat
{
    public override int GetMainStatValue(Rarity rarity, int level)
    {
        throw new Exception("Speed percentage should never be a mainstat");
    }

    public override void AddStats(Character character)
    {
        character.TotalSpeed += Value * 0.01f * character.BaseSpeed;
    }

    public override int GetMaximumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }

    public override int GetMinimumSubstatLevelIncrease(Rarity rarity)
    {
        throw new Exception("Percentage speed cannot be a main or substat in a gear");
    }
}